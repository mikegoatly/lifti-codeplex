// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying
{
    #region Using statements

    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Properties;
    using Lifti.Querying.Tokenization;

    #endregion

    /// <summary>
    /// The query parser capable of processing a LIFTI formatted query.
    /// </summary>

    public class LiftiQueryParser : IQueryParser
    {
        /// <summary>
        /// Parses the specified query text, returning a representative instance of <see cref="IFullTextQuery"/>.
        /// </summary>
        /// <param name="queryText">The query text to parse.</param>
        /// <param name="wordSplitter">The word splitter implementation used to split out individual words
        /// from composite statements.</param>
        /// <returns>The parsed query.</returns>
        public IFullTextQuery ParseQuery(string queryText, IWordSplitter wordSplitter)
        {
            IQueryPart rootPart = null;

            var state = new QueryParserState(queryText);
            QueryToken token;
            while (state.TryGetNextToken(out token))
            {
                rootPart = CreateQueryPart(state, token, wordSplitter, rootPart);
            }

            return new FullTextQuery(rootPart);
        }

        /// <summary>
        /// Creates an <see cref="IQueryPart"/> for the given token. Additional items may need to be read from the query parser state in order
        /// to achieve this.
        /// </summary>
        /// <param name="state">The query parser state.</param>
        /// <param name="token">The token to create the query part from.</param>
        /// <param name="wordSplitter">The word splitter to use when parsing text tokens.</param>
        /// <param name="rootPart">The current query root, in this context.</param>
        /// <returns>The created <see cref="IQueryPart"/> instance.</returns>
        private static IQueryPart CreateQueryPart(QueryParserState state, QueryToken token, IWordSplitter wordSplitter, IQueryPart rootPart)
        {
            switch (token.TokenType)
            {
                case QueryTokenType.Text:
                    return CreateWordParts(token, wordSplitter).Aggregate(rootPart, ComposePart);

                case QueryTokenType.BeginAdjacentTextOperator:
                    var adjacentWords = (from t in state.GetTokensUntil(QueryTokenType.EndAdjacentTextOperator)
                                         from p in CreateWordParts(t, wordSplitter)
                                         select p).ToArray();

                    return adjacentWords.Length > 0
                        ? ComposePart(rootPart, new AdjacentWordsQueryPart(adjacentWords))
                        : rootPart;

                case QueryTokenType.AndOperator:
                case QueryTokenType.OrOperator:
                case QueryTokenType.PrecedingNearOperator:
                case QueryTokenType.NearOperator:
                case QueryTokenType.PrecedingOperator:
                    var rightPart = CreateQueryPart(state, state.GetNextToken(), wordSplitter, null);
                    return CombineParts(rootPart, rightPart, token.TokenType);

                case QueryTokenType.OpenBracket:
                    var bracketedPart = state.GetTokensUntil(QueryTokenType.CloseBracket)
                        .Aggregate((IQueryPart)null, (current, next) => CreateQueryPart(state, next, wordSplitter, current));

                    return bracketedPart == null
                               ? rootPart
                               : ComposePart(rootPart, new BracketedQueryPart(bracketedPart));

                default:
                    throw new QueryParserException(Resources.UnexpectedTokenEncountered, token.TokenType);
            }
        }

        /// <summary>
        /// Creates the word parts from the given <see cref="QueryToken"/>. Typically this will
        /// yield only one word, but the given word splitter may yield more than one.
        /// </summary>
        /// <param name="token">The token to create the parts from.</param>
        /// <param name="wordSplitter">The word splitter to use.</param>
        /// <returns>The created <see cref="IWordQueryPart"/> instances.</returns>
        private static IEnumerable<IWordQueryPart> CreateWordParts(QueryToken token, IWordSplitter wordSplitter)
        {
            var useLike = token.TokenText.Length > 0 && token.TokenText[token.TokenText.Length - 1] == '*';
            return from w in wordSplitter.SplitWords(token.TokenText)
                   let word = w.Word
                   select useLike ? (IWordQueryPart)new LikeWordQueryPart(word) : new ExactWordQueryPart(word);
        }

        /// <summary>
        /// Composes the given parts together. If <paramref name="existingPart"/> is null then this will
        /// return the value in <paramref name="newPart"/>, otherwise the two parts will be combined
        /// with an <see cref="AndQueryOperator"/>.
        /// </summary>
        /// <param name="existingPart">The existing token.</param>
        /// <param name="newPart">The new token.</param>
        /// <returns>The composed part.</returns>
        private static IQueryPart ComposePart(IQueryPart existingPart, IQueryPart newPart)
        {
            if (existingPart == null)
            {
                return newPart;
            }

            return CombineParts(existingPart, newPart, QueryTokenType.AndOperator);
        }

        /// <summary>
        /// Combines the given <see cref="IQueryPart"/> instances with an operator of the specified type. If the <paramref name="existingPart"/>
        /// is an <see cref="IBinaryQueryOperator"/> then the precedence of it and the new operator will be inspected to determine execution
        /// order.
        /// </summary>
        /// <param name="existingPart">The existing part.</param>
        /// <param name="newPart">The new part.</param>
        /// <param name="operatorType">The type of the operator to use when combining the parts.</param>
        /// <returns>The <see cref="IBinaryQueryOperator"/> that combines the two parts.</returns>
        private static IBinaryQueryOperator CombineParts(IQueryPart existingPart, IQueryPart newPart, QueryTokenType operatorType)
        {
            if (existingPart == null)
            {
                throw new QueryParserException(Resources.UnexpectedOperator, operatorType);
            }

            var existingBinaryOperator = existingPart as IBinaryQueryOperator;
            if (existingBinaryOperator != null)
            {
                if (existingBinaryOperator.Precedence >= TokenPrecedence(operatorType))
                {
                    existingBinaryOperator.Right = CreateOperator(operatorType, existingBinaryOperator.Right, newPart);
                    return existingBinaryOperator;
                }

                return CreateOperator(operatorType, existingBinaryOperator, newPart);
            }

            return CreateOperator(operatorType, existingPart, newPart);
        }

        /// <summary>
        /// Creates the operator part for the given token type.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="leftPart">The left part to store in the operator part.</param>
        /// <param name="rightPart">The right part to store in the operator part.</param>
        /// <returns>The constructed <see cref="IBinaryQueryOperator"/></returns>
        private static IBinaryQueryOperator CreateOperator(QueryTokenType tokenType, IQueryPart leftPart, IQueryPart rightPart)
        {
            switch (tokenType)
            {
                case QueryTokenType.AndOperator:
                    return new AndQueryOperator(leftPart, rightPart);

                case QueryTokenType.OrOperator:
                    return new OrQueryOperator(leftPart, rightPart);

                case QueryTokenType.NearOperator:
                    return new NearQueryOperator(leftPart, rightPart);

                case QueryTokenType.PrecedingNearOperator:
                    return new PrecedingNearQueryOperator(leftPart, rightPart);

                case QueryTokenType.PrecedingOperator:
                    return new PrecedingQueryOperator(leftPart, rightPart);

                default:
                    throw new QueryParserException(Resources.UnexpectedOperatorInternal, tokenType);
            }
        }

        /// <summary>
        /// Gets the operator precedence for the given token type.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns>The relevant <see cref="OperatorPrecedence"/> value.</returns>
        private static OperatorPrecedence TokenPrecedence(QueryTokenType tokenType)
        {
            switch (tokenType)
            {
                case QueryTokenType.AndOperator:
                    return OperatorPrecedence.And;

                case QueryTokenType.OrOperator:
                    return OperatorPrecedence.Or;

                case QueryTokenType.PrecedingNearOperator:
                case QueryTokenType.NearOperator:
                case QueryTokenType.PrecedingOperator:
                    return OperatorPrecedence.Locational;

                default:
                    throw new QueryParserException(Resources.UnexpectedOperatorInternal, tokenType);
            }
        }

        /// <summary>
        /// A small helper class to maintain the current parser state - specifically the enumerator.
        /// </summary>
        private class QueryParserState
        {
            /// <summary>
            /// The enumerator for the parsed query tokens.
            /// </summary>
            private readonly IEnumerator<QueryToken> enumerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="LiftiQueryParser.QueryParserState"/> class.
            /// </summary>
            /// <param name="queryText">The query text.</param>
            public QueryParserState(string queryText)
            {
                this.enumerator = new QueryTokenizer().ParseQueryTokens(queryText).GetEnumerator();
            }

            /// <summary>
            /// Tries to get the next token from the enumerator.
            /// </summary>
            /// <param name="token">Returns the next token from the enumerator.</param>
            /// <returns><c>true</c> if the next token was successfully returned, otherwise <c>false</c>.</returns>
            public bool TryGetNextToken(out QueryToken token)
            {
                if (this.enumerator.MoveNext())
                {
                    token = this.enumerator.Current;
                    return true;
                }

                token = default(QueryToken);
                return false;
            }

            /// <summary>
            /// Gets the next token, raising an exception if there are no more items available.
            /// </summary>
            /// <returns>The next <see cref="QueryToken"/>.</returns>
            public QueryToken GetNextToken()
            {
                if (this.enumerator.MoveNext())
                {
                    return this.enumerator.Current;
                }

                throw new QueryParserException(Resources.UnexpectedEndOfQuery);
            }

            /// <summary>
            /// Gets tokens from the token stream until the specified token type is encountered. The terminating
            /// token will be consumed but not returned. If the terminating token is not encountered before the 
            /// end of the token stream, a <see cref="QueryParserException"/> will be thrown.
            /// </summary>
            /// <param name="terminatingToken">The terminating token.</param>
            /// <returns>The tokens that appear before the terminating token.</returns>
            public IEnumerable<QueryToken> GetTokensUntil(QueryTokenType terminatingToken)
            {
                var matchedTerminator = false;
                while (this.enumerator.MoveNext())
                {
                    if (this.enumerator.Current.TokenType == terminatingToken)
                    {
                        matchedTerminator = true;
                        break;
                    }

                    yield return this.enumerator.Current;
                }

                if (!matchedTerminator)
                {
                    throw new QueryParserException(Resources.ExpectedToken, terminatingToken);
                }
            }
        }
    }
}