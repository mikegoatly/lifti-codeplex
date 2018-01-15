// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using System;
    using System.Linq;

    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using Moq;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests for reading the original data file extent from a <see cref="LogFileManager"/> instance.
    /// </summary>
    [TestFixture]
    public class ReadingOriginalDataFileExtent : LogFileManagerTest
    {
        /// <summary>
        /// When reading the original extent value, subsequent writes resume from where previous writes left off.
        /// </summary>
        [Test]
        public void ShouldNotAffectLogLocation()
        {
            var dataFile = new Mock<IDataFileManager>(MockBehavior.Strict);
            dataFile.SetupGet(d => d.CurrentLength).Returns(233);
            dataFile.Setup(d => d.ReadRaw(10, It.Is<byte[]>(b => b.Length == 20))).Returns((int i, byte[] b) => b);
            dataFile.Setup(d => d.ReadRaw(30, It.Is<byte[]>(b => b.Length == 10))).Returns((int i, byte[] b) => b);

            this.Sut.InitializeNewLog(dataFile.Object);
            this.Sut.CurrentLength.ShouldEqual(Data.LogHeaderSize);

            this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 10, 20);
            this.Sut.CurrentLength.ShouldEqual(Data.LogHeaderSize + 20 + Data.LogEntryHeaderSize);

            var originalExtent = this.Sut.OriginalDataFileExtent;
            originalExtent.ShouldEqual(233);

            this.Sut.LogDataFrom(LogEntryDataType.PageHeader, dataFile.Object, 30, 10);

            this.Sut.CurrentLength.ShouldEqual(Data.LogHeaderSize + 30 + (Data.LogEntryHeaderSize * 2));
        }

        /// <summary>
        /// The correct extent value should be read from the correct location.
        /// </summary>
        [Test]
        public void ShouldReturnCorrectExtentValue()
        {
            var headerBytes = Data.LogFileHeader(TransactionLogState.Incomplete, 233).ToArray();
            this.SetInitialData(headerBytes);

            var originalExtent = this.Sut.OriginalDataFileExtent;
            originalExtent.ShouldEqual(233);
        }

        /// <summary>
        /// An exception should be thrown if no transaction is logged and the original extent is read.
        /// </summary>
        [Test]
        public void ShouldThrowExceptionIfNoTransactionLogged()
        {
            int originalExtent;
            Assert.Throws<InvalidOperationException>(
                () => originalExtent = this.Sut.OriginalDataFileExtent,
                "Unable to read original data file extent from log - no extent data logged.");
        }
    }
}
