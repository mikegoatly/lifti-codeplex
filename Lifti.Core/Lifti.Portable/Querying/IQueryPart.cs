// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// The interface implemented by all parts of a full text query.
    /// </summary>
    public interface IQueryPart
    {
        /// <summary>
        /// Executes this query part instance against the specified query context.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>The query result that contains the matched items.</returns>
        IQueryResult<TItem> ExecuteQuery<TItem>(IQueryContext<TItem> context);
    }
}
