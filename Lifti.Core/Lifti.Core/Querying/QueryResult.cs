// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Information about items returned from a query.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Instances of this struct are not compared")]
    public struct QueryResult<TItem> : IQueryResult<TItem>
    {
        /// <summary>
        /// The matches represented by this query result.
        /// </summary>
        private readonly IEnumerable<ItemWordMatch<TItem>> matches;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult&lt;TItem&gt;"/> struct.
        /// </summary>
        /// <param name="matches">The matches.</param>
        public QueryResult(IEnumerable<ItemWordMatch<TItem>> matches)
        {
            this.matches = matches;
        }

        /// <summary>
        /// Gets the matches covered by this query result.
        /// </summary>
        /// <value>The resulting matches.</value>
        public IEnumerable<ItemWordMatch<TItem>> Matches => this.matches;

        /// <summary>
        /// Unions this and the specified instance - this is the equivalent of an OR statement.
        /// </summary>
        /// <param name="results">The results to union this instance with.</param>
        /// <returns>The query result that represents the union of this and the specified instance.</returns>
        public IQueryResult<TItem> Union(IQueryResult<TItem> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return new QueryResult<TItem>(this.Matches.Union(results.Matches, ItemWordMatchEqualityComparer<TItem>.Instance));
        }

        /// <summary>
        /// Intersects this and the specified instance - this is the equivalent of an AND statement.
        /// </summary>
        /// <param name="results">The results to intersect this instance with.</param>
        /// <returns>The query result that represents the intersect of this and the specified instance.</returns>
        public IQueryResult<TItem> Intersect(IQueryResult<TItem> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            return new QueryResult<TItem>(this.Matches.Intersect(results.Matches, ItemWordMatchEqualityComparer<TItem>.Instance));
        }
    }
}
