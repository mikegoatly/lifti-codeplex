// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    using System;
    using System.IO;

    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for getting a data writer from the IO manager.
    /// </summary>
    [TestClass]
    public class GettingDataWriter : UnitTestBase
    {
        /// <summary>
        /// The path to the test file.
        /// </summary>
        private string path;

        /// <summary>
        /// Cleans up before and after each test.
        /// </summary>
        [TestCleanup]
        [TestInitialize]
        public void TestCleanup()
        {
            this.path = $"testindex{Guid.NewGuid()}.dat";
            FileHelper.DeleteIfExistsAsync(this.path).GetAwaiter().GetResult();
        }

        /// <summary>
        /// A data writer should be returned with its length set to the requested length.
        /// </summary>
        [TestMethod]
        public void ShouldReturnWriterWithCorrectExtent()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                using (var writer = manager.GetDataWriter(2, 2))
                {
                    Assert.IsTrue(writer.BaseStream.CanWrite);
                    Assert.AreEqual(0, writer.BaseStream.Position);
                    Assert.AreEqual(2, writer.BaseStream.Length); 
                }
            }
        }

        /// <summary>
        /// An exception should be thrown if an offset of less than 0 is requested.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfOffsetLessThanZeroRequested()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentOutOfRangeException>(() => manager.GetDataWriter(-1, 2), "Specified argument was out of the range of valid values.\r\nParameter name: fileOffset");
            }
        }

        /// <summary>
        /// An exception should be thrown if an offset of greater than the file length is requested.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfOffsetGreaterThanFileLengthRequested()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentOutOfRangeException>(() => manager.GetDataWriter(4, 2), "Specified argument was out of the range of valid values.\r\nParameter name: fileOffset");
            }
        }

        /// <summary>
        /// An exception should be thrown if an extent of greater than the file length is requested.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfRequestedExtentWouldOverwriteEndOfFile()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentOutOfRangeException>(() => manager.GetDataWriter(2, 3), "File is not currently large enough to accommodate the required length of data.\r\nParameter name: requiredLength");
            }
        }
    }
}
