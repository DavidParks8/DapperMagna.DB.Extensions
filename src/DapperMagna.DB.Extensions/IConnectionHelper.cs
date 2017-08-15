using System;
using System.Data;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions
{
    /// <summary>
    /// Exposes <see cref="IDbConnection"/> without having to worry about connection lifetime.
    /// With this interface, basic connection creation and disposal is abstracted away so
    /// that one can focus on their business logic.
    /// </summary>
    public interface IConnectionHelper
    {
        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task ExecuteAsync(Func<IDbConnection, Task> action);

        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>, and return a result.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> that returns a <typeparamref name="T"/>.</returns>
        Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action);

        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>, committing upon success,
        /// and rolling back upon exception.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action);

        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>, committing upon success,
        /// and rolling back upon exception.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <param name="isolationLevel">The isolation level to use in the internal transaction.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action, IsolationLevel isolationLevel);

        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>, committing upon success,
        /// rolling back upon exception, and return a result.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> that returns a <typeparamref name="T"/>.</returns>
        Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action);

        /// <summary>
        /// Interact with a database through a supplied <see cref="IDbConnection"/>, committing upon success,
        /// rolling back upon exception, and return a result.
        /// </summary>
        /// <param name="action">A function which executes queries against the <see cref="IDbConnection"/>.</param>
        /// <param name="isolationLevel">The isolation level to use in the internal transaction.</param>
        /// <typeparam name="T">The return type.</typeparam>
        /// <returns>An awaitable <see cref="Task"/> that returns a <typeparamref name="T"/>.</returns>
        Task<T> ExecuteWithRollbackOnFailureAsync<T>(Func<IDbConnection, Task<T>> action, IsolationLevel isolationLevel);
    }
}