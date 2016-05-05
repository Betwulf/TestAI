using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TestAICore.Repositories
{
    public interface IDocCollection<T>
    {

        string CollectionId { get; }

        string DatabaseLink { get; }
        

        string SelfLink
        {
            get;
        }




        IQueryable<T> GetAll();



        T Get(Expression<Func<T, bool>> predicate);

        T GetById(string id);

        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        Task<T> CreateAsync(T entity);


        Task<Document> UpdateAsync(string id, T entity);

        Task DeleteAsync(string id);

    }
}
