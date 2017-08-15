using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions
{
    public interface IConnectionHelper
    {
        Task ExecuteAsync(Func<IDbConnection, Task> action);

        Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action);

        Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action);

        Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action, IsolationLevel isolationLevel);

        Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action);

        Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action, IsolationLevel isolationLevel);
    }
}