// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// The various levels of operator precedence, in order.
    /// </summary>
    public enum OperatorPrecedence
    {
        /// <summary>
        /// The location-specific statements (near, before, after).
        /// </summary>
        Locational,

        /// <summary>
        /// And statements.
        /// </summary>
        And,

        /// <summary>
        /// Or statements.
        /// </summary>
        Or
    }

    /// <summary>
    /// The interface implemented by query parts that act as binary operations.
    /// </summary>
    public interface IBinaryQueryOperator : IQueryPart
    {
        /// <summary>
        /// Gets or sets the left part of the query operator.
        /// </summary>
        /// <value>The left query part.</value>
        IQueryPart Left { get; set;  }

        /// <summary>
        /// Gets or sets the right part of the query operator.
        /// </summary>
        /// <value>The right query part.</value>
        IQueryPart Right { get; set; }

        /// <summary>
        /// Gets the precedence of this operator. This value dictates how binary operators will be combined.
        /// </summary>
        /// <value>The operator precedence.</value>
        OperatorPrecedence Precedence { get; }
    }
}