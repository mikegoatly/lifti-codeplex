// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    #region Using statements

    using System;
    using System.IO;
    using System.Threading.Tasks;

    #endregion

    /// <summary>
    /// The base class for file managers.
    /// </summary>
    internal abstract class FileManagerBase : IFileManager
    {
        /// <summary>
        /// The thread synchronization object for this instance.
        /// </summary>
        protected readonly object SyncObj = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileManagerBase"/> class.
        /// </summary>
        /// <param name="stream">The stream being used to manage the file.</param>
        protected FileManagerBase(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException(nameof(stream), "Must be able to read from the stream.");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException(nameof(stream), "Must be able to write to the stream.");
            }

            this.DataStream = stream;
        }

        /// <summary>
        /// Gets the current length of the data managed by the IOManager.
        /// </summary>
        /// <value>
        /// The current length of the data.
        /// </value>
        public int CurrentLength => (int)this.DataStream.Length;

        /// <summary>
        /// Gets a value indicating whether this instance created the data file.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is created the data file for the first time; otherwise, <c>false</c>.
        /// </value>
        public bool IsNewFile => this.DataStream.Length == 0L;

        /// <summary>
        /// Gets the underlying data stream for the file.
        /// </summary>
        protected Stream DataStream { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly", Justification = "This is inherited by common classes that implement IDisposable.")]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Reads a chunk of raw data from the file.
        /// </summary>
        /// <param name="fileOffset">The file offset to read from.</param>
        /// <param name="data">The buffer to read the data into.</param>
        /// <returns>
        /// The data buffer that was passed in the <paramref name="data"/> parameter.
        /// </returns>
        public byte[] ReadRaw(int fileOffset, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            lock (this.SyncObj)
            {
                this.DataStream.Position = fileOffset;
                this.DataStream.Read(data, 0, data.Length);
                return data;
            }
        }

        /// <summary>
        /// Writes the raw data to the file at the given offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="length">The length of data to write from the data buffer.</param>
        public void WriteRaw(int fileOffset, byte[] data, int length)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            lock (this.SyncObj)
            {
                this.DataStream.Position = fileOffset;
                this.DataStream.Write(data, 0, length);
                this.DataStream.Flush();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.DataStream != null)
                {
                    this.DataStream.Flush();
                    this.DataStream.Dispose();
                    this.DataStream = null;
                }
            }
        }
    }
}