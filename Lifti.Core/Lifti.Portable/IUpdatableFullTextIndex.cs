// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// The interface implemented by full text indexes capable of supporting the
    /// removal and updating of items.
    /// </summary>
    /// <typeparam name="TKey">The type of the key to store in the index. This is the type that will
    /// be retrieved from the index.</typeparam>
    public interface IUpdatableFullTextIndex<TKey> : IFullTextIndex<TKey>
    {
        /// <summary>
        /// Determines whether the index currently has the specified item key in its index.
        /// </summary>
        /// <param name="itemKey">The item key to check for.</param>
        /// <returns>
        ///   <c>true</c> if the index contains the specified item; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(TKey itemKey);

        /// <summary>
        /// Removes the specified item key from the index. This has no effect if the item is not held in 
        /// the index.
        /// </summary>
        /// <param name="itemKey">The item to remove from the index.</param>
        void Remove(TKey itemKey);

        /// <summary>
        /// Removes the specified item keys from the index. Any items that are not contained in the index
        /// are ignored.
        /// </summary>
        /// <param name="itemKeys">The item keys to remove from the index.</param>
        void Remove(IEnumerable<TKey> itemKeys);
    }
}
