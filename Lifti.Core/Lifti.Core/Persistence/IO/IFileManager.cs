// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    /// <summary>
    /// The base interface for file manager interfaces.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Gets the current length of the data managed by the IOManager.
        /// </summary>
        /// <value>
        /// The current length of the data.
        /// </value>
        int CurrentLength { get; }

        /// <summary>
        /// Gets a value indicating whether this instance created the data file.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is created the data file for the first time; otherwise, <c>false</c>.
        /// </value>
        bool IsNewFile { get; }

        /// <summary>
        /// Reads a chunk of raw data from the file.
        /// </summary>
        /// <param name="fileOffset">The file offset to read from.</param>
        /// <param name="data">The buffer to read the data into.</param>
        /// <returns>The data buffer that was passed in the <paramref name="data"/> parameter.</returns>
        byte[] ReadRaw(int fileOffset, byte[] data);

        /// <summary>
        /// Writes the raw data to the file at the given offset.
        /// </summary>
        /// <param name="fileOffset">The file offset to write to.</param>
        /// <param name="data">The data to write.</param>
        /// <param name="length">The length of data to write from the data buffer.</param>
        void WriteRaw(int fileOffset, byte[] data, int length);
    }
}