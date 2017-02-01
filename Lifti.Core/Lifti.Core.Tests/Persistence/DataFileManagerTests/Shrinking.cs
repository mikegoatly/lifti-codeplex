// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.IOManagerTests
{
    using System;
    using System.IO;

    using Lifti.Persistence.IO;
    using Lifti.Tests.Persistence.DataFileManagerTests;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for extending the underlying file using the IO manager.
    /// </summary>
    [TestClass]
    public class Extending : UnitTestBase
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
        /// The underlying file should be shrunk as requested.
        /// </summary>
        [TestMethod]
        public void ShouldShrinkTheLengthOfTheUnderlyingFile()
        {
            FileHelper.CreateFile(this.path, "Testing");

            using (var manager = new DataFileManager(this.path))
            {
                manager.ShrinkStream(4);
            }

            Assert.AreEqual(4, FileHelper.GetFileBytes(this.path).Length);
        }

        /// <summary>
        /// An exception should be thrown if the file is shrunk to less than 1.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfRequestedLengthIsEqualToZero()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentException>(() => manager.ShrinkStream(0), "Required length must be greater than 0.\r\nParameter name: requiredLength");
            }
        }

        /// <summary>
        /// An exception should be thrown if the file is shrunk to less than 0.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfRequestedLengthIsLessThanZero()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentException>(() => manager.ShrinkStream(-10), "Required length must be greater than 0.\r\nParameter name: requiredLength");
            }
        }
    }
}
