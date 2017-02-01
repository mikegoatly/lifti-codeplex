// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Locking
{
    /// <summary>
    /// An ILock implementation that does nothing - this is used when locking is disabled, allowing
    /// the FullTextIndex implementation to work in the same manner (with the using pattern) regardless
    /// as to whether locking is enabled.
    /// </summary>
    internal struct NullLock : ILock
    {
        /// <inheritdoc />
        public void Dispose()
        {
            // No-op for a NullLock instance.
        }
    }
}
