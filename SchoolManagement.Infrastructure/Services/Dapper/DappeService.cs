using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SchoolManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services.Dapper
{
    public class DappeService : IDapperService
    {
        private readonly string _connectionString;
        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed;

        public DappeService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");
        }

        // Get or create an open connection
        public IDbConnection GetOpenConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        // Begin transaction (synchronous - IDbTransaction doesn't have async)
        public IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already exists.");

            var connection = GetOpenConnection();
            _transaction = connection.BeginTransaction(isolationLevel);
            return _transaction;
        }

        #region Query Methods (Connection-per-query pattern)

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class
        {
            // Always create new connection for thread safety
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryAsync<T>(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<T> QuerySingleOrDefaultAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<int> ExecuteAsync(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.ExecuteAsync(sql, parameters, transaction, commandType: commandType);
        }

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return await connection.QueryMultipleAsync(sql, parameters, transaction, commandType: commandType);
        }

        #endregion Query Methods (Connection-per-query pattern)

        #region Your Original Methods (Backward Compatibility)

        public Task<IEnumerable<T>> GetDataWithParameterAsync<T>(object parameters, string storedProcedureName)
            where T : class
            => QueryAsync<T>(storedProcedureName, parameters, CommandType.StoredProcedure);

        public Task<IEnumerable<T>> GetDataWithoutParameterAsync<T>(string storedProcedureName)
            where T : class
            => QueryAsync<T>(storedProcedureName, commandType: CommandType.StoredProcedure);

        public Task<IEnumerable<T>> GetDataBySqlCommandAsync<T>(string sqlString)
            where T : class
            => QueryAsync<T>(sqlString, commandType: CommandType.Text);

        public Task<int> ExecuteStoredProcedureAsync(object parameters, string storedProcedureName)
            => ExecuteAsync(storedProcedureName, parameters, CommandType.StoredProcedure);

        #endregion Your Original Methods (Backward Compatibility)

        #region Dispose Pattern

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _connection?.Dispose();
                }
                _connection = null;
                _transaction = null;
                _disposed = true;
            }
        }

        #endregion Dispose Pattern
    }
}