// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    /// <summary>
    /// A PRECEDING binary operator that performs an intersection of two results sets based on locality of the results.
    /// </summary>
    public class PrecedingQueryOperator : PositionalWordsQueryPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrecedingQueryOperator"/> class.
        /// </summary>
        /// <param name="left">The left part of the query operator.</param>
        /// <param name="right">The right part of the query operator.</param>
        public PrecedingQueryOperator(IQueryPart left, IQueryPart right)
            : base(left, right, 0, int.MaxValue)
        {   
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "(" + this.Left + " PRECEDING " + this.Right + ")";
        }
    }
}
