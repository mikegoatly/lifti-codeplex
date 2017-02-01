// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;

    /// <summary>
    /// The interface implemented by a context specific to a currently executing query.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IQueryContext<TKey>
    {
        /// <summary>
        /// Matches the given search word in the index, returning the items that match exactly or start with it.
        /// </summary>
        /// <param name="searchWord">The search word.</param>
        /// <returns>The matched items.</returns>
        IEnumerable<ItemWordMatch<TKey>> MatchWord(string searchWord);

        /// <summary>
        /// Matches the given search word in the index, returning the items that match it exactly.
        /// </summary>
        /// <param name="searchWord">The search word.</param>
        /// <returns>The matched items.</returns>
        IEnumerable<ItemWordMatch<TKey>> MatchWordExact(string searchWord);
    }
}
