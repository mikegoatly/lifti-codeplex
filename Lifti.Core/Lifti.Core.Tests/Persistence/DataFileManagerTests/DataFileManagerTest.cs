namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    #region Using statements

    using System.IO;
    using System.Text;

    using Lifti.Persistence.IO;

    using NUnit.Framework;

    #endregion

    public class DataFileManagerTest
    {
        internal MemoryStream Stream { get; private set; }

        internal DataFileManager Sut { get; private set; }

        [SetUp]
        public void SetUp()
        {
            this.Stream = new MemoryStream();
            this.SetInitialData("Test");
        }

        [TearDown]
        public void TearDown()
        {
            this.Sut?.Dispose();
            this.Stream?.Dispose();
        }

        protected void SetInitialData(string data)
        {
            this.Stream.SetLength(0L);
            var bytes = Encoding.UTF8.GetBytes(data);
            this.Stream.Write(bytes, 0, bytes.Length);
            this.Stream.Position = 0L;

            this.Sut = new DataFileManager(this.Stream);
        }
    }
}
