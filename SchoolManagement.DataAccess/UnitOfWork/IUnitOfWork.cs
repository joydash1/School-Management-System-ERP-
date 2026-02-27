using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        // Repository access
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        // Specific Repositories (for custom queries)
        IUserRepository Users { get; }

        IRoleRepository Roles { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // Transaction management
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // Additional useful methods
        bool HasActiveTransaction { get; }

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

        // Bulk Operations
        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);
    }
}