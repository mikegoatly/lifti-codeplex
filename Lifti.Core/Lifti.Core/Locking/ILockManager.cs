// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Locking
{
    using System;

    /// <summary>
    /// The interface implemented by a class capable of managing read/write locks for
    /// a full text index.
    /// </summary>
    public interface ILockManager : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether locking is enabled. This defaults to <c>true</c> - setting 
        /// it to <c>false</c> will result in slightly increased performance, but should be done only 
        /// it can be guaranteed that requests to the index will always come from the same thread.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        /// Occurs when a read lock has been acquired.
        /// </summary>
        event EventHandler ReadLockAcquired;

        /// <summary>
        /// Occurs when a read lock has been released.
        /// </summary>
        event EventHandler ReadLockReleased;

        /// <summary>
        /// Occurs when a write lock has been acquired.
        /// </summary>
        event EventHandler WriteLockAcquired;

        /// <summary>
        /// Occurs when a write lock has been released.
        /// </summary>
        event EventHandler WriteLockReleased;

        /// <summary>
        /// Obtains a read lock. This will remain active until the provided lock is disposed. Multiple threads
        /// are able to obtain read locks simultaneously.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ILock"/> that represents the read lock.
        /// </returns>
        ILock AcquireReadLock();

        /// <summary>
        /// Obtains a write lock. This will remain active until the provided lock is disposed. Only one write lock
        /// can be obtained at any one time. Any outstanding read locks will be processed before the write lock is
        /// granted, and any read locks subsequently requested will be queued until the write lock is released.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ILock"/> that represents the write lock.
        /// </returns>
        ILock AcquireWriteLock();
    }
}
