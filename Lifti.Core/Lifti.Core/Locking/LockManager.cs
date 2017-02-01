// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Locking
{
    using System;
    using System.Threading;

    /// <summary>
    /// The implementation of a lock manager for a full text index. This implements a multiple reader, single writer
    /// type of locking behaviour.
    /// </summary>

    public class LockManager : ILockManager
    {
        /// <summary>
        /// The reader/writer lock instance for this lock manager. No recursion is allowed
        /// </summary>
        private ReaderWriterLockSlim readerWriterLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LockManager"/> class.
        /// </summary>
        public LockManager()
            : this(LockRecursionPolicy.NoRecursion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LockManager"/> class.
        /// </summary>
        /// <param name="lockRecursionPolicy">The lock recursion policy.</param>
        public LockManager(LockRecursionPolicy lockRecursionPolicy)
        {
            this.Enabled = true;
            this.readerWriterLock = new ReaderWriterLockSlim(lockRecursionPolicy);
        }

        /// <inheritdoc />
        public bool Enabled
        {
            get;
            set;
        }

        /// <inheritdoc />
        public event EventHandler ReadLockAcquired;

        /// <inheritdoc />
        public event EventHandler ReadLockReleased;

        /// <inheritdoc />
        public event EventHandler WriteLockAcquired;

        /// <inheritdoc />
        public event EventHandler WriteLockReleased;

        /// <inheritdoc />
        public ILock AcquireReadLock()
        {
            if (this.Enabled)
            {
                // Enter the lock
                this.readerWriterLock.EnterReadLock();

                this.OnReadLockAcquired();

                // Return a read lock instance
                return new ReadLock(this);
            }

            // Locking is not enabled, so return a new disposable null lock that does nothing
            return new NullLock();
        }

        /// <inheritdoc />
        public ILock AcquireWriteLock()
        {
            if (this.Enabled)
            {
                // Enter the lock
                this.readerWriterLock.EnterWriteLock();

                this.OnWriteLockAcquired();

                // Return a new instance of a write lock
                return new WriteLock(this);
            }

            // Locking is not enabled, so return a new disposable null lock that does nothing
            return new NullLock();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases a read lock.
        /// </summary>
        internal void ReleaseReadLock()
        {
            this.OnReadLockReleased();
            this.readerWriterLock.ExitReadLock();
        }

        /// <summary>
        /// Releases a write lock.
        /// </summary>
        internal void ReleaseWriteLock()
        {
            this.OnWriteLockReleased();
            this.readerWriterLock.ExitWriteLock();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.readerWriterLock != null)
                {
                    this.readerWriterLock.Dispose();
                    this.readerWriterLock = null;
                }
            }
        }

        /// <summary>
        /// Called when a read lock has been acquired.
        /// </summary>
        protected virtual void OnReadLockAcquired()
        {
            this.ReadLockAcquired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when a read lock has been released.
        /// </summary>
        protected virtual void OnReadLockReleased()
        {
            this.ReadLockReleased?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when a write lock has been acquired.
        /// </summary>
        protected virtual void OnWriteLockAcquired()
        {
            this.WriteLockAcquired?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called when a write lock has been released.
        /// </summary>
        protected virtual void OnWriteLockReleased()
        {
            this.WriteLockReleased?.Invoke(this, EventArgs.Empty);
        }
    }
}
