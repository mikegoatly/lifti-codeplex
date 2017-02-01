// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System.Collections.Generic;
    using System.Linq;

    #endregion

    /// <summary>
    /// A type-safe implementation of a full text query that can be executed against a full text index.
    /// </summary>
    public class FullTextQuery : IFullTextQuery
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextQuery"/> class.
        /// </summary>
        public FullTextQuery()
        {    
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullTextQuery"/> class.
        /// </summary>
        /// <param name="root">The root query part for the query.</param>
        public FullTextQuery(IQueryPart root)
        {
            this.Root = root;
        }

        /// <summary>
        /// Gets or sets the root query part of this instance. This may be a simple one word
        /// query part, or a complex hierarchical query part.
        /// </summary>
        /// <value>The root query part.</value>
        public IQueryPart Root
        {
            get;
            set;
        }

        /// <summary>
        /// Executes this instance against the given full text index, returning an enumerable set
        /// if matching items.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="context">The query context to execute the query against.</param>
        /// <returns>Enumerable list of matching results.</returns>
        public IEnumerable<TItem> Execute<TItem>(IQueryContext<TItem> context)
        {
            if (this.Root == null)
            {
                // Handle the empty query case.
                return Enumerable.Empty<TItem>();
            }

            return from r in this.Root.ExecuteQuery(context).Matches
                   select r.Item;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Root == null ? string.Empty : this.Root.ToString();
        }
    }
}
