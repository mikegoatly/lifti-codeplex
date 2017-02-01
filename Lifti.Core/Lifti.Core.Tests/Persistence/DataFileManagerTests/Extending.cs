// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Lifti.Persistence.IO;

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
        /// The underlying file should be extended as requested.
        /// </summary>
        [TestMethod]
        public void ShouldExtendTheLengthOfTheUnderlyingFile()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                manager.ExtendStream(8);
            }

            Assert.AreEqual(8, FileHelper.GetFileBytes(this.path).Length);
        }

        /// <summary>
        /// An exception should be thrown if the file is extended to less than its current size.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfRequestedLengthIsLessThanOrEqualToCurrentLength()
        {
            FileHelper.CreateFile(this.path, "Test");

            using (var manager = new DataFileManager(this.path))
            {
                AssertRaisesException<ArgumentException>(() => manager.ExtendStream(4), "Can only extend file beyond its current length.\r\nParameter name: requiredLength");
            }
        }
    }
}
