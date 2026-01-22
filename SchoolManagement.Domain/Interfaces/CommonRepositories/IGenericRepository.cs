using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Domain.Interfaces.CommonRepositories
{
    public interface IGenericRepository<T> where T : class
    {
        // GET operations (Async only - remove sync methods)
        Task<T> GetAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            CancellationToken cancellationToken = default);

        // Pagination (essential for production)
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        // Queryable for complex queries
        IQueryable<T> GetQueryable();

        // EXISTS check
        Task<bool> ExistsAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default);

        // CREATE operations (Async only)
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        // UPDATE operations
        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        // DELETE operations
        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}