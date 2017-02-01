// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// A binary writer that operates over a pooled data resource. When it is disposed the data buffer is returned 
    /// to the data pool.
    /// </summary>
    internal class PooledDataBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// The offset in the underlying stream that the data should be written back to.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// The pooled data resource the reader is operating over.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// The underlying file manager that should be written back to when this instance is disposed.
        /// </summary>
        private IFileManager fileManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledDataBinaryWriter"/> class.
        /// </summary>
        /// <param name="fileManager">The underlying file manager that should be written back to when this instance is disposed.</param>
        /// <param name="offset">The offset in the stream that the data should be written to.</param>
        /// <param name="data">The pooled data resource the reader will release back to the <see cref="DataPool"/> when this instance
        /// is disposed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is disposed by the data writer when it gets disposed")]
        public PooledDataBinaryWriter(IFileManager fileManager, int offset, byte[] data)
            : base(new MemoryStream(data))
        {
            this.offset = offset;
            this.fileManager = fileManager;
            this.data = data;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.BinaryReader"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (this.data != null)
            {
                ((MemoryStream)this.BaseStream).Flush();

                // Optimisation - only write out the amount of data that was written to this instance
                // this is opposed to writing out data equal to the size of the actual underlying data buffer.
                var position = (int)this.BaseStream.Position;
                this.fileManager.WriteRaw(this.offset, this.data, position);

                // Re-pool the data block
                DataPool.PoolData(this.data);
                this.data = null;
                this.fileManager = null;
            }

            base.Dispose(disposing);
        }
    }
}
