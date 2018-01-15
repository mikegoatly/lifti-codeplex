namespace Lifti.Tests.Persistence.LogFileManagerTests
{
    #region Using statements

    using System.IO;

    using Lifti.Persistence.IO;

    using NUnit.Framework;

    #endregion

    public class LogFileManagerTest
    {
        internal MemoryStream Stream { get; private set; }

        internal LogFileManager Sut { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Stream = new MemoryStream();
            this.Sut = new LogFileManager(this.Stream);
        }

        [TearDown]
        public void TearDown()
        {
            this.Sut?.Dispose();
            this.Stream?.Dispose();
        }

        protected void SetInitialData(byte[] bytes)
        {
            this.Stream.SetLength(0L);
            this.Stream.Write(bytes, 0, bytes.Length);
            this.Stream.Position = 0L;

            this.Sut = new LogFileManager(this.Stream);
        }
    }
}
