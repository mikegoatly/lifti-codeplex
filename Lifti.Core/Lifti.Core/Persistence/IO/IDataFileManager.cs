// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// The interface implemented by classes capable of handling interactions with an underlying data stream
    /// </summary>
    public interface IDataFileManager : IFileManager, IDisposable
    {
        /// <summary>
        /// Gets a data writer set up to write at the given file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to position the writer at.</param>
        /// <param name="requiredLength">The maximum length of data that will be written, starting at the given offset.</param>
        /// <returns>The <see cref="BinaryWriter"/>, with its location set to the relevant position.</returns>
        BinaryWriter GetDataWriter(int fileOffset, int requiredLength);

        /// <summary>
        /// Gets a data reader set up to read at the given file offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to position the reader at.</param>
        /// <param name="requiredLength">The maximum length of data that will be read from the given offset.</param>
        /// <returns>
        /// The <see cref="BinaryReader"/>, with its location set to the relevant position.
        /// </returns>
        BinaryReader GetDataReader(int fileOffset, int requiredLength);

        /// <summary>
        /// Extends the underlying stream so that it is as long as the given required length.
        /// </summary>
        /// <param name="requiredLength">The required length of the underlying stream.</param>
        void ExtendStream(int requiredLength);

        /// <summary>
        /// Shrinks the underlying stream so that it is the given required length.
        /// </summary>
        /// <param name="requiredLength">The required length of the underlying stream.</param>
        void ShrinkStream(int requiredLength);
    }
}