using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DapperMagna.DB.Extensions.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    [TestCategory("Unit")]
    public class DefaultConnectionHelperTests
    {
        [TestMethod]
        public void ConstructorShouldThrowWhenPassedNullConnectionFactory()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultConnectionHelper(null));
        }

        [TestMethod]
        public async Task ExecuteShouldThrowWhenActionIsNull()
        {
            var connectionHelper = new DefaultConnectionHelper(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => connectionHelper.ExecuteAsync(null));
        }

        [TestMethod]
        public async Task ExecuteWithReturnShouldThrowWhenActionIsNull()
        {
            var connectionHelper = new DefaultConnectionHelper(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => connectionHelper.ExecuteAsync<int>(null));
        }

        [TestMethod]
        public async Task ExecuteWithRollbackShouldThrowWhenActionIsNull()
        {
            var connectionHelper = new DefaultConnectionHelper(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => connectionHelper.ExecuteWithRollbackOnFailureAsync(null));
        }

        [TestMethod]
        public async Task ExecuteWithRollbackAndReturnShouldThrowWhenActionIsNull()
        {
            var connectionHelper = new DefaultConnectionHelper(() => null);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => connectionHelper.ExecuteWithRollbackOnFailureAsync<int>(null));
        }

        [TestMethod]
        public async Task ExecuteShouldDisposeConnection()
        {
            var connection = NewOpenConnection();
            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await connectionHelper.ExecuteAsync(dbConnection => Task.CompletedTask);

            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteWithResultShouldDisposeConnection()
        {
            var connection = NewOpenConnection();
            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await connectionHelper.ExecuteAsync<int>(dbConnection => Task.FromResult(0));

            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteWithRollbackShouldCommitOnSuccess()
        {
            var connection = NewOpenConnection();
            var transaction = NewTransaction();

            connection.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object);
            transaction.Setup(x => x.Commit());

            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await connectionHelper.ExecuteWithRollbackOnFailureAsync(dbConnection => Task.CompletedTask);

            transaction.Verify(x => x.Commit(), Times.Once);
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteWithRollbackAndResultShouldCommitOnSuccess()
        {
            var connection = NewOpenConnection();
            var transaction = NewTransaction();

            connection.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object);
            transaction.Setup(x => x.Commit());

            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await connectionHelper.ExecuteWithRollbackOnFailureAsync<int>(dbConnection => Task.FromResult(0));

            transaction.Verify(x => x.Commit(), Times.Once);
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteWithRollbackShouldRollbackOnFailure()
        {
            var connection = NewOpenConnection();
            var transaction = NewTransaction();

            connection.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object);
            transaction.Setup(x => x.Rollback());

            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await Assert.ThrowsExceptionAsync<Exception>(
                () => connectionHelper.ExecuteWithRollbackOnFailureAsync(
                    dbConnection => Task.FromException(new Exception())));

            transaction.Verify(x => x.Rollback(), Times.Once);
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public async Task ExecuteWithRollbackAndResultShouldRollbackOnFailure()
        {
            var connection = NewOpenConnection();
            var transaction = NewTransaction();

            connection.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction.Object);
            transaction.Setup(x => x.Rollback());

            var connectionHelper = new DefaultConnectionHelper(() => connection.Object);
            await Assert.ThrowsExceptionAsync<Exception>(
                () => connectionHelper.ExecuteWithRollbackOnFailureAsync<int>(
                    dbConnection => Task.FromException<int>(new Exception())));

            transaction.Verify(x => x.Rollback(), Times.Once);
            connection.Verify(x => x.Dispose(), Times.Once);
        }

        [TestMethod]
        public void OpenConnectionShouldNotBeOpenedAgain()
        {
            var connection = NewConnection();
            connection.Setup(x => x.State).Returns(ConnectionState.Open);

            var testHelper = new TestConnectionHelper();
            testHelper.TestOpenState(connection.Object);
            connection.Setup(x => x.State).Returns(ConnectionState.Connecting);

            testHelper.TestOpenState(connection.Object);
            connection.Verify(x => x.Open(), Times.Never);
        }

        [TestMethod]
        public void ConnectionShouldBeOpenedSynchronously()
        {
            var connection = NewConnection();
            connection.Setup(x => x.State).Returns(ConnectionState.Closed);
            connection.Setup(x => x.Open());

            var testHelper = new TestConnectionHelper();
            testHelper.TestOpenState(connection.Object);

            connection.Verify(x => x.Open(), Times.Once);
        }

        [TestMethod]
        public void GuaranteeOpenConnectionShouldThrowWhenConnectionIsNull()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new TestConnectionHelper().TestOpenState(null));
        }

        private static Mock<IDbTransaction> NewTransaction()
        {
            var transaction = new Mock<IDbTransaction>(MockBehavior.Strict);
            transaction.Setup(x => x.Dispose());
            return transaction;
        }

        private static Mock<IDbConnection> NewOpenConnection()
        {
            var connection = NewConnection();
            connection.Setup(x => x.State).Returns(ConnectionState.Open);
            return connection;
        }

        private static Mock<IDbConnection> NewConnection()
        {
            var connection = new Mock<IDbConnection>(MockBehavior.Strict);
            connection.Setup(x => x.Dispose());
            return connection;
        }

        private class TestConnectionHelper : DefaultConnectionHelper
        {
            public TestConnectionHelper()
                : base(() => null)
            {
            }

            public void TestOpenState(IDbConnection connection)
            {
                GuaranteeOpenState(connection);
            }
        }
    }
}
