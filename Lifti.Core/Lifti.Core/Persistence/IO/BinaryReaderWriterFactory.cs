// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    /// <summary>
    /// A factory class capable of creating binary readers and writers for which the underlying
    /// data is managed by the <see cref="DataPool"/>.
    /// </summary>
    internal static class BinaryReaderWriterFactory
    {
        /// <summary>
        /// Creates a binary reader from the file manager at the given offset, for the given length.
        /// </summary>
        /// <param name="fileManager">The file manager.</param>
        /// <param name="offset">The offset at which to read the data from.</param>
        /// <param name="length">The length of data to read.</param>
        /// <returns>The binary reader for the data.</returns>
        public static PooledDataBinaryReader CreateReader(IFileManager fileManager, int offset, int length)
        {
            var data = DataPool.AllocateData(length);

            fileManager.ReadRaw(offset, data);
            
            return new PooledDataBinaryReader(data);
        }

        /// <summary>
        /// Creates a binary writer from the file manager at the given offset, for the given length.
        /// </summary>
        /// <param name="fileManager">The file manager.</param>
        /// <param name="offset">The offset at which to write the data to.</param>
        /// <param name="length">The maximum length of data to writer.</param>
        /// <returns>The binary writer for the data.</returns>
        public static PooledDataBinaryWriter CreateWriter(IFileManager fileManager, int offset, int length)
        {
            var data = DataPool.AllocateData(length);
            return new PooledDataBinaryWriter(fileManager, offset, data);
        }
    }
}
