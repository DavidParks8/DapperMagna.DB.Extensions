using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions
{
    public class DefaultConnectionHelper : IConnectionHelper
    {
        private readonly Func<IDbConnection> _connectionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConnectionHelper"/> class.
        /// </summary>
        /// <param name="connectionFactory">A factory function that supplies an <see cref="IDbConnection"/></param>
        public DefaultConnectionHelper(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes.
        /// </summary>
        /// <param name="action">A function which executes queries against the connection.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task ExecuteAsync(Func<IDbConnection, Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var connection = _connectionFactory())
            {
                await action(connection);
            }
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes,
        /// returning the action's result.
        /// </summary>
        /// <param name="action">A function which executes queries against the connection, and returns a <typeparamref name="T"/>.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var connection = _connectionFactory())
            {
                return await action(connection);
            }
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

            using (var connection = _connectionFactory())
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

            using (var connection = _connectionFactory())
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
    }
}
