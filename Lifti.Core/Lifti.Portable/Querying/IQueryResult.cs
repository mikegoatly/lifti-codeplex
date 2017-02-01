// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Information about items returned from a query.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public interface IQueryResult<TItem>
    {
        /// <summary>
        /// Gets the matches covered by this query result.
        /// </summary>
        /// <value>The resulting matches.</value>
        IEnumerable<ItemWordMatch<TItem>> Matches { get; }

        /// <summary>
        /// Unions this and the specified instance - this is the equivalent of an OR statement.
        /// </summary>
        /// <param name="results">The results to union this instance with.</param>
        /// <returns>The query result that represents the union of this and the specified instance.</returns>
        IQueryResult<TItem> Union(IQueryResult<TItem> results);

        /// <summary>
        /// Intersects this and the specified instance - this is the equivalent of an AND statement.
        /// </summary>
        /// <param name="results">The results to intersect this instance with.</param>
        /// <returns>The query result that represents the intersect of this and the specified instance.</returns>
        IQueryResult<TItem> Intersect(IQueryResult<TItem> results);
    }
}
