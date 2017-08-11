// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    #region Using statements

    using System;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests for getting a data writer from the IO manager.
    /// </summary>
    [TestFixture]
    public class GettingDataWriter : DataFileManagerTest
    {
        /// <summary>
        /// An exception should be thrown if an offset of greater than the file length is requested.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfOffsetGreaterThanFileLengthRequested()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Sut.GetDataReader(4, 2), "Specified argument was out of the range of valid values.\r\nParameter name: fileOffset");
        }

        /// <summary>
        /// An exception should be thrown if an offset of less than 0 is requested.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfOffsetLessThanZeroRequested()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Sut.GetDataReader(-1, 2), "Specified argument was out of the range of valid values.\r\nParameter name: fileOffset");
        }

        /// <summary>
        /// An exception should be thrown if an extent of greater than the file length is requested.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfRequestedExtentWouldOverwriteEndOfFile()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => this.Sut.GetDataReader(2, 3), "File is not currently large enough to accommodate the required length of data.\r\nParameter name: requiredLength");
        }

        /// <summary>
        /// A data writer should be returned with its length set to the requested length.
        /// </summary>
        [Test]
        public void ShouldReturnWriterWithCorrectExtent()
        {
            using (var writer = this.Sut.GetDataWriter(2, 2))
            {
                writer.BaseStream.CanWrite.ShouldBeTrue();
                writer.BaseStream.Position.ShouldEqual(0L);
                writer.BaseStream.Length.ShouldEqual(2L);
            }
        }
    }
}
