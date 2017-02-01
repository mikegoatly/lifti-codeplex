// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Locking
{
    using System;

    /// <summary>
    /// The interface implemented by a lock obtained in an index.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "This enforces a pattern with the ILockManager methods")]
    public interface ILock : IDisposable
    {}
}
