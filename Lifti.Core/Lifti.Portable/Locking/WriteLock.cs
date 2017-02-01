// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Locking
{
    /// <summary>
    /// A write lock obtained on a full text index.
    /// </summary>
    internal struct WriteLock : ILock
    {
        /// <summary>
        /// The lock manager for this instance.
        /// </summary>
        private readonly LockManager lockManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="WriteLock"/> struct.
        /// </summary>
        /// <param name="lockManager">The lock manager.</param>
        internal WriteLock(LockManager lockManager)
        {
            this.lockManager = lockManager;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.lockManager.ReleaseWriteLock();
        }
    }
}
