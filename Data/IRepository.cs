using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);

        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate, string sortColumnName, int skipCount, int takeCount, OrderType orderByType);

        int Count(Expression<Func<T, bool>> predicate);

        T Get(Expression<Func<T, bool>> predicate);

        void Add(T entity);

        void Update(Expression<Func<T, bool>> predicate, T entity);

        void Delete(Expression<Func<T, bool>> predicate, bool forceDelete = false);

        bool Any(Expression<Func<T, bool>> predicate);

        List<string> GetFieldList(string field);

    }
    public enum OrderType
    {
        ASC,
        DESC
    }
}
