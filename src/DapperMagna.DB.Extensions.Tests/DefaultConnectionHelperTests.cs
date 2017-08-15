using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
