// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogFactoryTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for creating a new <see cref="TransactionLogRollback"/> instance using a <see cref="TransactionLogFactory"/>.
    /// </summary>
    [TestClass]
    public class CreatingNewTransactionLogRollback
    {
        /// <summary>
        /// The factory should successfully create a rollback instance..
        /// </summary>
        [TestMethod]
        public void ShouldCreateRollbackInstance()
        {
            var logFile = new Mock<ILogFileManager>();
            var dataFile = new Mock<IDataFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(p => p.FileManager).Returns(dataFile.Object);

            var rollback = factory.CreateTransactionLogRollback(pageManager.Object);
            Assert.IsNotNull(rollback);

            pageManager.VerifyAll();
        }
    }
}
