// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// The Input/Output manager responsible for interactions with the underlying data stream.
    /// </summary>
    internal class DataFileManager : FileManagerBase, IDataFileManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFileManager"/> class.
        /// </summary>
        /// <param name="dataFileName">The data file path.</param>
        public DataFileManager(string dataFileName)
            : base(dataFileName)
        {
            // TODO - Verify the file header here, writing it out if required
        }

        /// <summary>
        /// Gets the size of a page, in bytes.
        /// </summary>
        internal const int PageSize = 8192;

        /// <summary>
        /// The size of the validation header information at the start of the persisted file.
        /// </summary>
        internal const int HeaderSize = 8;

        /// <summary>
        /// The size of header data allocated to the page manager.
        /// </summary>
        internal const int PageManagerHeaderSize = 24;

        /// <summary>
        /// The size of header data allocated each page.
        /// </summary>
        internal const int PageHeaderSize = 21;

        /// <summary>
        /// The total space allocated in the header of the file before page data begins.
        /// </summary>
        internal const int TotalHeaderSize = HeaderSize + PageManagerHeaderSize;

        /// <summary>
        /// Gets the file offset for the given physical page number.
        /// </summary>
        /// <param name="pageNumber">
        /// The physical page number.
        /// </param>
        /// <returns>
        /// The offset into the persisted file at which the page starts.
        /// </returns>
        public static int GetPageOffset(int pageNumber)
        {
            checked
            {
                return TotalHeaderSize + (pageNumber * PageSize);
            }
        }

        /// <summary>
        /// Gets a data writer set up to write at the given file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to position the writer at.</param>
        /// <param name="requiredLength">The maximum length of data that will be written, starting at the given offset.</param>
        /// <returns>
        /// The <see cref="BinaryWriter"/>, with its location set to the relevant position.
        /// </returns>
        public BinaryWriter GetDataWriter(int fileOffset, int requiredLength)
        {
            Debug.WriteLine("Getting data writer at {0} length {1}", fileOffset, requiredLength);

            var currentLength = this.DataStream.Length;
            if (fileOffset < 0 || fileOffset >= currentLength)
            {
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            }

            if (fileOffset + requiredLength > currentLength)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredLength), "File is not currently large enough to accommodate the required length of data.");
            }

            return BinaryReaderWriterFactory.CreateWriter(this, fileOffset, requiredLength);
        }

        /// <summary>
        /// Gets a data reader set up to read at the given file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to position the reader at.</param>
        /// <param name="requiredLength">The maximum length of data that will be read from the given offset.</param>
        /// <returns>
        /// The <see cref="BinaryReader"/>, with its location set to the relevant position.
        /// </returns>
        public BinaryReader GetDataReader(int fileOffset, int requiredLength)
        {
            var currentLength = this.DataStream.Length;
            if (fileOffset < 0 || fileOffset >= currentLength)
            {
                throw new ArgumentOutOfRangeException(nameof(fileOffset));
            }

            if (fileOffset + requiredLength > currentLength)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredLength), "File is not currently large enough to accommodate the required length of data.");
            }

            return BinaryReaderWriterFactory.CreateReader(this, fileOffset, requiredLength);
        }

        /// <summary>
        /// Extends the underlying stream so that it is as long as the given required length.
        /// </summary>
        /// <param name="requiredLength">The required length of the underlying stream.</param>
        public void ExtendStream(int requiredLength)
        {
            if (this.DataStream.Length >= requiredLength)
            {
                throw new ArgumentException("Can only extend file beyond its current length.", nameof(requiredLength));
            }

            this.DataStream.SetLength(requiredLength);
        }

        /// <summary>
        /// Shrinks the underlying stream so that it is the given required length.
        /// </summary>
        /// <param name="requiredLength">The required length of the underlying stream.</param>
        public void ShrinkStream(int requiredLength)
        {
            if (requiredLength <= 0)
            {
                throw new ArgumentException("Required length must be greater than 0.", nameof(requiredLength));
            }

            this.DataStream.SetLength(requiredLength);
        }
    }
}
