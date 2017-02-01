// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying.Tokenization
{
    /// <summary>
    /// The various types of query tokens that can be encountered.
    /// </summary>
    public enum QueryTokenType
    {
        /// <summary>
        /// The token contains text to search on.
        /// </summary>
        Text,

        /// <summary>
        /// The token is an And operator.
        /// </summary>
        AndOperator,

        /// <summary>
        /// The token is an Or operator.
        /// </summary>
        OrOperator,

        /// <summary>
        /// The token is an open bracket.
        /// </summary>
        OpenBracket,

        /// <summary>
        /// The token is a close bracket.
        /// </summary>
        CloseBracket,

        /////// <summary>
        /////// The token is a Not operator. NOT IMPLEMENTED
        /////// </summary>
        ////NotOperator,

        /// <summary>
        /// The token indicates that all subsequent text tokens should appear immediately next to each other.
        /// </summary>
        BeginAdjacentTextOperator,

        /// <summary>
        /// The token indicates an end to adjacent text tokens.
        /// </summary>
        EndAdjacentTextOperator,

        /// <summary>
        /// The token is a Near operator - the words must be near each other in either direction.
        /// </summary>
        NearOperator,

        /// <summary>
        /// The token is a Preceding Near operator - the left word must be near and precede the right word.
        /// </summary>
        PrecedingNearOperator,

        /// <summary>
        /// The token is a preceding operator - the left word must precede the right word.
        /// </summary>
        PrecedingOperator
    }

    /// <summary>
    /// A token parsed from a query.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Instances of this struct are not compared")]
    public struct QueryToken
    {
        /// <summary>
        /// The token text.
        /// </summary>
        private readonly string tokenText;

        /// <summary>
        /// The type of the token.
        /// </summary>
        private readonly QueryTokenType tokenType;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryToken"/> struct.
        /// </summary>
        /// <param name="tokenText">The token text.</param>
        /// <param name="tokenType">The type of the token.</param>
        public QueryToken(string tokenText, QueryTokenType tokenType)
        {
            this.tokenText = tokenText;
            this.tokenType = tokenType;
        }

        /// <summary>
        /// Gets the token text.
        /// </summary>
        /// <value>The token text.</value>
        public string TokenText
        {
            get { return this.tokenText; }
        }

        /// <summary>
        /// Gets the type of the token.
        /// </summary>
        /// <value>The type of the token.</value>
        public QueryTokenType TokenType
        {
            get { return this.tokenType; }
        }
    }
}
