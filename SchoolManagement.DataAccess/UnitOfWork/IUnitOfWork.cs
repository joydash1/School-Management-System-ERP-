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

        // Transaction management
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}