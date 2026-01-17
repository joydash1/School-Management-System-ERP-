using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services.Dapper
{
    public interface IDapperService : IDisposable
    {
        // Query methods
        Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class;

        Task<T> QueryFirstOrDefaultAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class;

        Task<T> QuerySingleOrDefaultAsync<T>(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null)
            where T : class;

        Task<int> ExecuteAsync(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null);

        Task<SqlMapper.GridReader> QueryMultipleAsync(
            string sql,
            object parameters = null,
            CommandType commandType = CommandType.Text,
            IDbTransaction transaction = null);

        // Transaction management (synchronous)
        IDbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        // Connection management
        IDbConnection GetOpenConnection();

        // Your original methods for backward compatibility
        Task<IEnumerable<T>> GetDataWithParameterAsync<T>(object parameters, string storedProcedureName) where T : class;

        Task<IEnumerable<T>> GetDataWithoutParameterAsync<T>(string storedProcedureName) where T : class;

        Task<IEnumerable<T>> GetDataBySqlCommandAsync<T>(string sqlString) where T : class;

        Task<int> ExecuteStoredProcedureAsync(object parameters, string storedProcedureName);
    }
}