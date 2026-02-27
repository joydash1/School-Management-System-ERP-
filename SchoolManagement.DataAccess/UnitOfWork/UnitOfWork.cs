using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolManagement.DataAccess.DataContext;
using SchoolManagement.DataAccess.Repositories;
using SchoolManagement.Domain.Interfaces.AuthenticationAndAuthorization;
using SchoolManagement.Domain.Interfaces.CommonRepositories;
using SchoolManagement.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories;
        private IDbContextTransaction _transaction = null;

        private bool _disposed;

        public bool HasActiveTransaction => _transaction != null;

        // Specific repositories
        private IUserRepository? _userRepository;

        private IRoleRepository? _roleRepository;
        private IRefreshTokenRepository? _refreshTokenRepository;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _repositories = new Dictionary<Type, object>();
        }

        // Repository factory
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(type, repository);
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        // Specific Repositories
        public IUserRepository Users =>
            _userRepository ??= new UserRepository(_dbContext);

        public IRoleRepository Roles =>
            _roleRepository ??= new RoleRepository(_dbContext);

        public IRefreshTokenRepository RefreshTokens =>
            _refreshTokenRepository ??= new RefreshTokenRepository(_dbContext);

        // Transaction management
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync(cancellationToken);
                }

                // Detach all tracked entities
                foreach (var entry in _dbContext.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _dbContext.SaveChangesAsync(cancellationToken);

        public async Task<int> SaveChangesAndCommitAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                return await SaveChangesAsync(cancellationToken);
            }

            var result = await SaveChangesAsync(cancellationToken);
            await CommitAsync(cancellationToken);
            return result;
        }

        // Cleanup
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            await _dbContext.DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
    Func<Task<T>> operation,
    CancellationToken cancellationToken = default)
        {
            if (HasActiveTransaction)
            {
                return await operation();
            }

            await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
        }

        ///Custom Repository
        public TRepository GetCustomRepository<TRepository, TEntity>()
    where TRepository : class, IGenericRepository<TEntity>
    where TEntity : class
        {
            if (!typeof(TRepository).GetConstructors()
    .Any(c => c.GetParameters().Length == 1 &&
              c.GetParameters()[0].ParameterType == typeof(ApplicationDbContext)))
            {
                throw new InvalidOperationException(
                    $"Repository {typeof(TRepository).Name} must have a constructor that accepts ApplicationDbContext");
            }

            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                var repository = Activator.CreateInstance(typeof(TRepository), _dbContext);
                _repositories.Add(type, repository);
            }

            return (TRepository)_repositories[type];
        }

        // Execute raw SQL
        public async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }
    }
}