using Microsoft.EntityFrameworkCore;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
        }

        #region GET Operations

        public async Task<T> GetAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default)
            => await _dbSet.FirstOrDefaultAsync(expression, cancellationToken);

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (expression != null)
                query = query.Where(expression);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> expression = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            int pageIndex = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet;

            if (expression != null)
                query = query.Where(expression);

            var totalCount = await query.CountAsync(cancellationToken);

            if (orderBy != null)
                query = orderBy(query);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public IQueryable<T> GetQueryable()
            => _dbSet.AsQueryable();

        public async Task<bool> ExistsAsync(
            Expression<Func<T, bool>> expression,
            CancellationToken cancellationToken = default)
            => await _dbSet.AnyAsync(expression, cancellationToken);

        #endregion GET Operations

        #region CREATE Operations

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
            => await _dbSet.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
            => await _dbSet.AddRangeAsync(entities, cancellationToken);

        #endregion CREATE Operations

        #region UPDATE Operations

        public void Update(T entity)
            => _dbContext.Update(entity);

        public void UpdateRange(IEnumerable<T> entities)
            => _dbContext.UpdateRange(entities);

        #endregion UPDATE Operations

        #region DELETE Operations

        public void Remove(T entity)
            => _dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities)
            => _dbSet.RemoveRange(entities);

        #endregion DELETE Operations
    }
}