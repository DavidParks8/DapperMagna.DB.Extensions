using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions.Testing
{
    public class WeakConnectionHelper : IConnectionHelper
    {
        private readonly WeakReference<IDbConnection> _connection;

        public WeakConnectionHelper(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _connection = new WeakReference<IDbConnection>(connection);
        }

        public async Task ExecuteAsync(Func<IDbConnection, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            await action(ResolveConnection());
        }

        public async Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return await action(ResolveConnection());
        }

        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action)
        {
            await ExecuteWithRollbackOnFailureAsync(action, IsolationLevel.Snapshot);
        }

        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action, IsolationLevel isolationLevel)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var connection = ResolveConnection();
            using (var transaction = connection.BeginTransaction(isolationLevel))
            {
                try
                {
                    await action(connection);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            return await ExecuteWithRollbackOnFailureAsync(action, IsolationLevel.Snapshot);
        }

        public async Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action, IsolationLevel isolationLevel)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var connection = ResolveConnection();
            using (var transaction = connection.BeginTransaction(isolationLevel))
            {
                try
                {
                    var result = await action(connection);
                    transaction.Commit();
                    return result;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        protected IDbConnection ResolveConnection()
        {
            if (_connection.TryGetTarget(out var connection))
            {
                return connection;
            }

            throw new ObjectDisposedException(nameof(connection));
        }
    }
}
