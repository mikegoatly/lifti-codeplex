// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using System;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests the initialization of a new transaction on the <see cref="LogFileManager"/> class.
    /// </summary>
    [TestFixture]
    public class InitializingNewTransaction : LogFileManagerTest
    {
        /// <summary>
        /// The state of the log file should be marked as incomplete after initialization.
        /// </summary>
        [Test]
        public void ShouldMarkTheStateAsIncomplete()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(299);

            this.Sut.InitializeNewLog(dataFile.Object);

            this.Sut.LogState.ShouldEqual(TransactionLogState.Incomplete);
        }

        /// <summary>
        /// If a null data file manager is provided, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfNullDataFileManagerProvided()
        {
            Assert.Throws<ArgumentNullException>(() => this.Sut.InitializeNewLog(null), "Value cannot be null.\r\nParameter name: dataFileManager");
        }

        /// <summary>
        /// When initializing a new transaction, the expected data should be written out in the header.
        /// </summary>
        [Test]
        public void ShouldWriteExpectedDataOutInHeader()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(299);

            this.Sut.InitializeNewLog(dataFile.Object);

            this.Stream.ToArray().ShouldEqual(Data.LogFileHeader(TransactionLogState.Incomplete, 299));
        }
    }
}
