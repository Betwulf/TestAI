using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using System.Collections.Concurrent;

namespace TestAICore.Repositories
{
    class DocCollectionFile<T> : IDocCollection<T>
        where T : Document
    {
        private readonly string mRootDir;
        private ConcurrentDictionary<string, T> DocStore;
        private readonly bool isCached;

        public DocCollectionFile(string aRootDir)
        {
            DocStore = new ConcurrentDictionary<string, T>();
            mRootDir = aRootDir;
            string isCachedString = ConfigurationManager.AppSettings["docdb-caching-on"];
            if (isCachedString == "true") { isCached = true; } else { isCached = false; }
        }

        public string CollectionId
        { get { return this.GetType().GenericTypeArguments[0].FullName; } }

        public string DatabaseLink
        { get { return this.GetType().ToString(); } }

        public string SelfLink
        {
            // Should Remove and delete from interface too
            get { return this.GetType().ToString(); }
        }


        public string GetDir { get { return System.IO.Path.Combine(mRootDir, CollectionId); } }

        private string GetFilename(string id)
        {
            return System.IO.Path.Combine(GetDir, id + ".json");
        }

        public async Task<T> CreateAsync(T entity)
        {
            await Task.Run(() =>
            {

                string pathString = GetDir;
                Directory.CreateDirectory(pathString); // Create Directory if it isn't already made
                pathString = GetFilename(entity.Id);
                if (!File.Exists(pathString))
                {
                    Console.WriteLine("Saving New File \"{0}\" .", pathString);
                    File.WriteAllText(pathString, JsonConvert.SerializeObject(entity));
                    if (isCached)
                    {
                        DocStore.AddOrUpdate(entity.Id, entity, (k, v) => entity);
                    }
                }
                else
                {
                    Console.WriteLine("Overwriting File \"{0}\" .", pathString);
                    File.WriteAllText(pathString, JsonConvert.SerializeObject(entity));
                    if (isCached)
                    {
                        DocStore.AddOrUpdate(entity.Id, entity, (k, v) => entity);
                    }
                }
            });
            return entity;

        }

        public async Task DeleteAsync(string id)
        {
            await Task.Run(() =>
            {
                T val;
                if (isCached) DocStore.TryRemove(id, out val);
                string pathString = GetFilename(id);
                Console.WriteLine("Deleting file: {0}\n", pathString);
                if (File.Exists(pathString))
                {
                    File.Delete(pathString);
                }
                else
                {
                    Console.WriteLine("File \"{0}\" did not exist.", pathString);
                }
            });
        }

        public IEnumerable<T> Find(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public T Get(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> GetAll()
        {
            var dir = new DirectoryInfo(GetDir);
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.json", SearchOption.AllDirectories);
            List<T> tempList = new List<T>();
            foreach (var item in fileList)
            {
                T storedDoc = null;
                T tempObject = null;
                var id = item.Name.Remove(item.Name.Length - item.Extension.Length);
                if (isCached && DocStore.TryGetValue(id, out storedDoc))
                {
                    tempObject = storedDoc;
                }
                else
                {
                    tempObject = JsonConvert.DeserializeObject<T>(File.ReadAllText(item.FullName));
                    if (isCached)
                    {
                        DocStore.AddOrUpdate(id, tempObject, (k, v) => tempObject);
                    }
                }
                tempList.Add(tempObject);
            }
            return new EnumerableQuery<T>(tempList);
        }



        public T GetById(string id)
        {
            T storedDoc;
            if (isCached && DocStore.TryGetValue(id, out storedDoc))
            {
                //Console.WriteLine("returning stored doc: " + id);
                return storedDoc;
            }
            string pathString = GetDir;
            Directory.CreateDirectory(pathString); // Create Directory if it isn't already made
            pathString = GetFilename(id);
            if (File.Exists(pathString))
            {
                T theDoc = JsonConvert.DeserializeObject<T>(File.ReadAllText(pathString));
                if (isCached)
                {
                    DocStore.AddOrUpdate(id, theDoc, (k, v) => theDoc);
                }

                return theDoc;
            }
            else
            {
                return null;
            }
        }

        public async Task<Document> UpdateAsync(string id, T entity)
        {
            return await CreateAsync(entity);
        }

        public void DeleteDatabase()
        {
            var dir = new DirectoryInfo(GetDir);
            IEnumerable<FileInfo> fileList = dir.GetFiles("*.json", SearchOption.AllDirectories);
            List<T> tempList = new List<T>();
            foreach (var item in fileList)
            {
                File.Delete(item.FullName);
            }
        }
    }
}
