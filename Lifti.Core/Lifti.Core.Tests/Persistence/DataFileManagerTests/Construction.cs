// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    #region Using statements

    using System;

    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    /// Tests for the construction of the IO manager.
    /// </summary>
    [TestClass]
    public class Construction
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
        /// If the file doesn't exist when the IOManager is created, the IsNewFile property should
        /// return true.
        /// </summary>
        [TestMethod]
        public void ShouldReportNewFileCreation()
        {
            using (var manager = new DataFileManager(this.path))
            {
                Assert.IsTrue(manager.IsNewFile);
            }
        }

        /// <summary>
        /// If the file exists when the IOManager is created, the IsNewFile property should
        /// return false.
        /// </summary>
        [TestMethod]
        public void ShouldReportThatFileAlreadyExists()
        {
            using (var manager = new DataFileManager(this.path))
            {
                manager.ExtendStream(5);
                using (var writer = manager.GetDataWriter(0, 5))
                {
                    writer.Write("Test");
                    writer.Flush();
                }
            }

            using (var manager = new DataFileManager(this.path))
            {
                Assert.IsFalse(manager.IsNewFile);
            }
        }
    }
}
