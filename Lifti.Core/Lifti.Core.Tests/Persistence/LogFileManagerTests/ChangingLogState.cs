// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests for changing (and indirectly, reading) the state of the log file.
    /// </summary>
    [TestFixture]
    public class ChangingLogState : LogFileManagerTest
    {
        /// <summary>
        /// When the state is changed, the new value should be returned by the property.
        /// </summary>
        [Test]
        public void ChangedStatusShouldBeReturnedByTheProperty()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.None;

            this.Sut.LogState.ShouldEqual(TransactionLogState.None);
        }

        /// <summary>
        /// The state of the log file should be written to the log file.
        /// </summary>
        [Test]
        public void ShouldWriteStateToLogFile()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.LogState = TransactionLogState.None;

            var expectedData = Data.LogFileHeader(TransactionLogState.None, 233);

            var actual = this.Stream.ToArray();

            actual.ShouldEqual(expectedData);
        }
    }
}
