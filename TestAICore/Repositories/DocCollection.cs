using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TestAICore.Repositories
{
    public class DocCollection<T> : IDocCollection<T> 
        where T : Document
    {

        public string CollectionId { get; private set; }

        public string DatabaseLink { get; private set; }

        private DocumentCollection Collection { get; set; }

        private DocumentClient Client { get; set; }

        public DocCollection(DocumentClient client, string databaseLink, string collectionId)
        {
            DatabaseLink = databaseLink;
            CollectionId = collectionId;
            Client = client;

            Collection = GetOrCreateCollection(DatabaseLink, CollectionId);
        }

        public string SelfLink
        {
            get { return Collection.SelfLink; }
        }



        private DocumentCollection GetOrCreateCollection(string databaseLink, string collectionId)
        {
            var col = Client.CreateDocumentCollectionQuery(databaseLink)
                              .Where(c => c.Id == collectionId)
                              .AsEnumerable()
                              .FirstOrDefault();

            if (col == null)
            {
                col = Client.CreateDocumentCollectionAsync(databaseLink,
                    new DocumentCollection { Id = collectionId },
                    new RequestOptions { OfferType = "S1" }).Result;
            }
            return col;
        }

        public IQueryable<T> GetAll()
        {
            var s = from col in Client.CreateDocumentQuery<T>(Collection.SelfLink)
                    select col;
            return s;
        }



        public T Get(Expression<Func<T, bool>> predicate)
        {
            return Client.CreateDocumentQuery<T>(Collection.DocumentsLink)
                        .Where(predicate)
                        .AsEnumerable()
                        .FirstOrDefault();
        }

        public T GetById(string id)
        {
            T doc = Client.CreateDocumentQuery<T>(Collection.SelfLink)
                .Where(d => d.Id == id)
                .AsEnumerable()
                .FirstOrDefault();

            return doc;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            var ret = Client.CreateDocumentQuery<T>(Collection.SelfLink)
                .Where(predicate)
                .AsEnumerable();

            return ret;
        }

        public async Task<T> CreateAsync(T entity)
        {

            // Create Document in DocumentDB
            var queryDone = false;
            while (!queryDone)
            {
                try
                {
                    Document doc = await Client.CreateDocumentAsync(Collection.SelfLink, entity);
                    T ret = (T)(dynamic)doc;
                    return ret;
                }
                catch (DocumentClientException docEx)
                {
                    var statusCode = (int)docEx.StatusCode;
                    if (statusCode == 429 || statusCode == 503) //429 is "TooManyRequests", no idea what 503 is.
                        await Task.Delay(docEx.RetryAfter);
                    else
                        throw;
                }
                catch (AggregateException aggregateException)
                {
                    if (aggregateException.InnerException.GetType() == typeof(DocumentClientException))
                    {

                        var docExcep = aggregateException.InnerException as DocumentClientException;
                        var statusCode = (int)docExcep.StatusCode;
                        if (statusCode == 429 || statusCode == 503)
                            await Task.Delay(docExcep.RetryAfter);
                        else
                            throw;
                    }
                }
            }
            return null;


        }

        public async Task<Document> UpdateAsync(string id, T entity)
        {
            Document doc = GetById(id);
            return await Client.ReplaceDocumentAsync(doc.SelfLink, entity);
        }

        public async Task DeleteAsync(string id)
        {
            Document doc = GetById(id);
            await Client.DeleteDocumentAsync(doc.SelfLink);
        }


    }
}
