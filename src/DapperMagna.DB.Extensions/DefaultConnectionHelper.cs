using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions
{
    /// <summary>
    /// Exposes <see cref="T:System.Data.IDbConnection" /> without having to worry about connection lifetime.
    /// With this interface, basic connection creation and disposal is abstracted away so
    /// that one can focus on their business logic. Each method obtains an <see cref="T:System.Data.IDbConnection" />
    /// and disposes it inline.
    /// </summary>
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
                GuaranteeOpenState(connection);
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
                GuaranteeOpenState(connection);
                return await action(connection);
            }
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes,
        /// committing upon success, and rolling back upon exception.
        /// <remarks>No transactions should be created within the supplied <paramref name="action"/>.</remarks>
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action)
        {
            await ExecuteWithRollbackOnFailureAsync(action, IsolationLevel.Snapshot);
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes,
        /// committing upon success, and rolling back upon exception.
        /// <remarks>No transactions should be created within the supplied <paramref name="action"/>.</remarks>
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <param name="isolationLevel">The isolation level to use in the internal transaction.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action, IsolationLevel isolationLevel)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var connection = _connectionFactory())
            {
                GuaranteeOpenState(connection);
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
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes,
        /// committing upon success, rolling back upon exception, and return a result.
        /// <remarks>No transactions should be created within the supplied <paramref name="action"/>.</remarks>
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> that returns a <typeparamref name="T"/>.</returns>
        public async Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            return await ExecuteWithRollbackOnFailureAsync(action, IsolationLevel.Snapshot);
        }

        /// <summary>
        /// Supplies a fresh <see cref="IDbConnection"/> to your <paramref name="action"/> then disposes it when the action completes,
        /// committing upon success, rolling back upon exception, and return a result.
        /// <remarks>No transactions should be created within the supplied <paramref name="action"/>.</remarks>
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <param name="isolationLevel">The isolation level to use in the internal transaction.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> that returns a <typeparamref name="T"/>.</returns>
        public async Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action, IsolationLevel isolationLevel)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var connection = _connectionFactory())
            {
                GuaranteeOpenState(connection);
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

        /// <summary>
        /// Opens the <paramref name="connection"/> if it is not already.
        /// </summary>
        /// <param name="connection">A connection to open.</param>
        protected static void GuaranteeOpenState(IDbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.State != ConnectionState.Open
                && connection.State != ConnectionState.Connecting)
            {
                connection.Open();
            }
        }
    }
}
