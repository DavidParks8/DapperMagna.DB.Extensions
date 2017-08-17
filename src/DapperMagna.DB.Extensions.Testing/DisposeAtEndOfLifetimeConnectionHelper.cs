using System;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DapperMagna.DB.Extensions.Testing
{
    public class DisposeAtEndOfLifetimeConnectionHelper : IConnectionHelper, IDisposable
    {
        private IDbConnection _connection;
        private bool _disposed;

        public DisposeAtEndOfLifetimeConnectionHelper(IDbConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        ~DisposeAtEndOfLifetimeConnectionHelper()
        {
            Dispose(false);
        }

        public async Task ExecuteAsync(Func<IDbConnection, Task> action)
        {
            CheckDisposal();
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            await action(_connection);
        }

        public async Task<T> ExecuteAsync<T>(Func<IDbConnection, Task<T>> action)
        {
            CheckDisposal();
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return await action(_connection);
        }

        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action)
        {
            await ExecuteWithRollbackOnFailureAsync(action, IsolationLevel.Snapshot);
        }

        public async Task ExecuteWithRollbackOnFailureAsync(Func<IDbConnection, Task> action, IsolationLevel isolationLevel)
        {
            CheckDisposal();
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var transaction = _connection.BeginTransaction(isolationLevel))
            {
                try
                {
                    await action(_connection);
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
            CheckDisposal();
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            using (var transaction = _connection.BeginTransaction(isolationLevel))
            {
                try
                {
                    var result = await action(_connection);
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (disposing)
            {
                try
                {
                    _connection.Dispose();
                    _connection = null;
                }
                catch (Exception)
                {
                    // we don't want any exceptions escaping and crashing the app if this runs in a finalizer
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposal()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
