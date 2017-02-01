// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// The interface implemented by a type-safe full text query.
    /// </summary>
    public interface IFullTextQuery
    {
        /// <summary>
        /// Executes this instance against the given full text index, returning an enumerable set
        /// if matching items.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>Enumerable list of matching results.</returns>
        IEnumerable<TItem> Execute<TItem>(IQueryContext<TItem> context);
    }
}
