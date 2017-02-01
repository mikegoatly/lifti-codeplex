// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Querying.Tokenization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using Lifti.Properties;

    /// <summary>
    /// A tokenizer class capable of breaking a LIFTI query string down into its constituent tokens.
    /// </summary>
    public class QueryTokenizer
    {
        /// <summary>
        /// Parses the tokens from the given query text.
        /// </summary>
        /// <param name="queryText">The query text to parse.</param>
        /// <returns>An enumerable of parsed <see cref="QueryToken"/> instances.</returns>
        public IEnumerable<QueryToken> ParseQueryTokens(string queryText)
        {
            if (queryText == null)
            {
                throw new ArgumentNullException(nameof(queryText));
            }

            var state = QueryParserState.None;
            var parsedOperator = false;
            var processingString = false;
            var currentToken = new StringBuilder();
            foreach (var character in queryText)
            {
                switch (state)
                {
                    case QueryParserState.ProcessingStringAwaitingNextWord:
                    case QueryParserState.ProcessingString:
                        state = ProcessStringCharacter(state, character, currentToken);
                        
                        // Emit a word if now in the awaiting next word state
                        if (state == QueryParserState.ProcessingStringAwaitingNextWord && currentToken.Length > 0)
                        {
                            yield return CreateQueryToken(currentToken, parsedOperator);
                            currentToken.Length = 0;
                        }

                        break;

                    case QueryParserState.ProcessingOperator:
                        state = ProcessOperatorCharacter(state, character, currentToken);
                        break;

                    case QueryParserState.ProcessingWord:
                        state = ProcessWordCharacter(state, character, currentToken);
                        break;
                }

                if (state == QueryParserState.None)
                {
                    if (currentToken.Length > 0)
                    {
                        yield return CreateQueryToken(currentToken, parsedOperator);
                        currentToken.Length = 0;
                    }

                    if (processingString)
                    {
                        // This will only occur when a close quote has been encountered
                        yield return new QueryToken("\"", QueryTokenType.EndAdjacentTextOperator);
                        processingString = false;
                    }
                    else
                    {
                        state = ProcessDefaultState(state, character, currentToken);
                        processingString = state == QueryParserState.ProcessingString;
                        if (processingString)
                        {
                            yield return new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator);
                        }

                        parsedOperator = state == QueryParserState.ProcessingOperator;
                    }
                }
            }

            if (currentToken.Length > 0)
            {
                // Characters have been processed in the current token - this
                // is the last in the text, so ensure it is yielded
                yield return CreateQueryToken(currentToken, parsedOperator);
            }
        }

        /// <summary>
        /// Creates a query token from the given token text. The type of the token
        /// will vary depending on whether an operator has been parsed.
        /// </summary>
        /// <param name="currentToken">The current token.</param>
        /// <param name="parsedOperator">Set to <c>true</c> if an operator has been parsed.</param>
        /// <returns>The created <see cref="QueryToken"/> instance.</returns>
        private static QueryToken CreateQueryToken(StringBuilder currentToken, bool parsedOperator)
        {
            var text = currentToken.ToString();
            if (parsedOperator)
            {
                return new QueryToken(text, ConvertTextToOperatorType(text));
            }

            return new QueryToken(text, QueryTokenType.Text);
        }

        /// <summary>
        /// Converts the text to a <see cref="QueryTokenType"/>.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>The converted <see cref="QueryTokenType"/>.</returns>
        private static QueryTokenType ConvertTextToOperatorType(string text)
        {
            switch (text)
            {
                case "&":
                    return QueryTokenType.AndOperator;

                case "|":
                    return QueryTokenType.OrOperator;

                case "(":
                    return QueryTokenType.OpenBracket;

                case ")":
                    return QueryTokenType.CloseBracket;

                ////Not currently implemented.
                ////case "!":
                ////    return QueryTokenType.NotOperator;

                case "~>":
                    return QueryTokenType.PrecedingNearOperator;

                case ">>":
                    return QueryTokenType.PrecedingOperator;

                case "~":
                    return QueryTokenType.NearOperator;

                default:
                    throw new QueryParserException(string.Format(CultureInfo.CurrentCulture, Resources.UnknownOperator, text));
            }
        }

        /// <summary>
        /// Processes a character encountered within the default state of the parser.
        /// </summary>
        /// <param name="currentState">The current state of the parser.</param>
        /// <param name="character">The character being parsed.</param>
        /// <param name="currentToken">The current token text.</param>
        /// <returns>The new parser state.</returns>
        private static QueryParserState ProcessDefaultState(QueryParserState currentState, char character, StringBuilder currentToken)
        {
            switch (character)
            {
                case '&':
                case '|':
                case '>':
                case ')':
                case '(':
                case '~':
                ////case '!':
                    currentState = QueryParserState.ProcessingOperator;
                    currentToken.Append(character);
                    break;

                case '"':
                    currentState = QueryParserState.ProcessingString;
                    break;

                default:
                    if (!char.IsWhiteSpace(character))
                    {
                        currentState = QueryParserState.ProcessingWord;
                        currentToken.Append(character);
                    }

                    break;
            }

            return currentState;
        }

        /// <summary>
        /// Processes a character encountered within a word.
        /// </summary>
        /// <param name="currentState">The current state of the parser.</param>
        /// <param name="character">The character being parsed.</param>
        /// <param name="currentToken">The current token text.</param>
        /// <returns>The new parser state.</returns>
        private static QueryParserState ProcessWordCharacter(QueryParserState currentState, char character, StringBuilder currentToken)
        {
            switch (character)
            {
                //// Note that ! is excluded from this list because it would only be valid at the start of a word
                case '&':
                case '|':
                case '>':
                case ')':
                case '(':
                case '~':
                    currentState = QueryParserState.None;
                    break;

                default:
                    if (char.IsWhiteSpace(character))
                    {
                        currentState = QueryParserState.None;
                    }
                    else
                    {
                        currentToken.Append(character);
                    }

                    break;
            }

            return currentState;
        }

        /// <summary>
        /// Processes a character encountered within an operator.
        /// </summary>
        /// <param name="currentState">The current state of the parser.</param>
        /// <param name="character">The character being parsed.</param>
        /// <param name="currentToken">The current token text.</param>
        /// <returns>The new parser state.</returns>
        private static QueryParserState ProcessOperatorCharacter(QueryParserState currentState, char character, StringBuilder currentToken)
        {
            switch (character)
            {
                // The only characters that can be involved in tokens over 1 character in length are &gt; and ~
                case '>':
                case '~':
                    currentToken.Append(character);
                    break;

                default:
                    currentState = QueryParserState.None;
                    break;
            }

            return currentState;
        }

        /// <summary>
        /// Processes a character encountered within a string.
        /// </summary>
        /// <param name="currentState">The current state of the parser.</param>
        /// <param name="character">The character being parsed.</param>
        /// <param name="currentToken">The current token text.</param>
        /// <returns>The new parser state.</returns>
        private static QueryParserState ProcessStringCharacter(QueryParserState currentState, char character, StringBuilder currentToken)
        {
            if (character == '"')
            {
                if (currentToken.Length > 0 && currentToken[currentToken.Length - 1] == '\\')
                {
                    // This is an escaped quote - replace the escape character with the quote
                    currentToken.Length -= 1;
                    currentToken.Append(character);
                }
                else
                {
                    currentState = QueryParserState.None;
                }
            }
            else if (char.IsWhiteSpace(character))
            {
                currentState = QueryParserState.ProcessingStringAwaitingNextWord;   
            }
            else
            {
                currentState = QueryParserState.ProcessingString;
                currentToken.Append(character);
            }

            return currentState;
        }

        /// <summary>
        /// The various states the query parser may be in.
        /// </summary>
        private enum QueryParserState
        {
            /// <summary>
            /// The parser is not currently processing any element.
            /// </summary>
            None,

            /// <summary>
            /// The parser is processing a quoted string.
            /// </summary>
            ProcessingString,

            /// <summary>
            /// The parser is processing a quoted string and any word that has
            /// already been processed should be yielded.
            /// </summary>
            ProcessingStringAwaitingNextWord,

            /// <summary>
            /// The parser is processing an operator.
            /// </summary>
            ProcessingOperator,

            /// <summary>
            /// The parser is processing a word.
            /// </summary>
            ProcessingWord
        }
    }
}
