// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// A binary reader that operates over a pooled data resource. When it is disposed the data buffer is returned 
    /// to the data pool.
    /// </summary>
    internal class PooledDataBinaryReader : BinaryReader
    {
        /// <summary>
        /// The pooled data resource the reader is operating over.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledDataBinaryReader"/> class.
        /// </summary>
        /// <param name="data">The pooled data resource the reader will release back to the <see cref="DataPool"/> when this instance
        /// is disposed.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "This is disposed by the data reader when it gets disposed")]
        public PooledDataBinaryReader(byte[] data)
            : base(new MemoryStream(data))
        {
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
                DataPool.PoolData(this.data);
                this.data = null;
            }

            base.Dispose(disposing);
        }
    }
}
