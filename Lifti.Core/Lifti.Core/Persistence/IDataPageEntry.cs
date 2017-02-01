// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// The interface implemented by entries contained within data pages.
    /// </summary>
    public interface IDataPageEntry
    {
        /// <summary>
        /// Gets the unique id for the entry.
        /// </summary>
        /// <value>The unique id for the entry.</value>
        int Id { get; }

        /// <summary>
        /// Gets the size of the item contained in the entry, in bytes.
        /// </summary>
        /// <value>The size of the item in the entry.</value>
        short Size { get; }
    }
}