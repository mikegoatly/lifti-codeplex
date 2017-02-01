// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogRollbackTests
{
    using System;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the construction of a <see cref="TransactionLogRollback"/> instance.
    /// </summary>
    [TestClass]
    public class Construction : UnitTestBase
    {
        /// <summary>
        /// If a null log file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfNullLogFileManagerProvided()
        {
            var dataFileManager = new Mock<IDataFileManager>();

            AssertRaisesArgumentNullException(() => new TransactionLogRollback(null, dataFileManager.Object), "logFileManager");
        }

        /// <summary>
        /// If a null data file manager is provided, then a <see cref="ArgumentNullException"/> should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfNullDataFileManagerProvided()
        {
            var logFileManager = new Mock<ILogFileManager>();

            AssertRaisesArgumentNullException(() => new TransactionLogRollback(logFileManager.Object, null), "dataFileManager");
        }
    }
}
