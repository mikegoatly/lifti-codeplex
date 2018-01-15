// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    #region Using statements

    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests for the construction of the IO manager.
    /// </summary>
    [TestFixture]
    public class Construction : DataFileManagerTest
    {
        /// <summary>
        /// If the file doesn't exist when the IOManager is created, the IsNewFile property should
        /// return true.
        /// </summary>
        [Test]
        public void ShouldReportNewFileCreation()
        {
            this.SetInitialData("");
            this.Sut.IsNewFile.ShouldBeTrue();
        }

        /// <summary>
        /// If the file exists when the IOManager is created, the IsNewFile property should
        /// return false.
        /// </summary>
        [Test]
        public void ShouldReportThatFileAlreadyExists()
        {
            this.Sut.ExtendStream(5);
            using (var writer = this.Sut.GetDataWriter(0, 5))
            {
                writer.Write("Test");
                writer.Flush();
            }

            this.Stream.Position = 0L;
            using (var manager = new DataFileManager(this.Stream))
            {
                manager.IsNewFile.ShouldBeFalse();
            }
        }
    }
}
