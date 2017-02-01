// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.QueryTokenizerTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Querying;
    using Lifti.Querying.Tokenization;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the QueryTokenizer class.
    /// </summary>
    [TestClass]
    public class QueryTokenizerTest : UnitTestBase
    {
        /// <summary>
        /// Verifies that the tokenizer yields no results when passed an empty string.
        /// </summary>
        [TestMethod]
        public void EmptyStringYieldsNoResults()
        {
            var target = new QueryTokenizer();
            AssertResults(target.ParseQueryTokens(string.Empty));
        }

        /// <summary>
        /// Verifies that the tokenizer throws an argument null exception when given null query text.
        /// </summary>
        [TestMethod]
        public void NullStringThrowsException()
        {
            var target = new QueryTokenizer();
            AssertRaisesArgumentNullException(() => target.ParseQueryTokens(null).ToArray(), "queryText");
        }

        /// <summary>
        /// Verifies that the tokenizer yields one result when passed a string containing a single word with no spaces.
        /// </summary>
        [TestMethod]
        public void SingleWordYieldsOneResult()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens("Testing"),
                new QueryToken("Testing", QueryTokenType.Text));
        }

        /// <summary>
        /// Verifies that the tokenizer yields one result when passed a string containing a single word with spaces around it.
        /// </summary>
        [TestMethod]
        public void SingleWordWithSpacePaddingYieldsOneResult()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens("  \t  Testing   \t "),
                new QueryToken("Testing", QueryTokenType.Text));
        }

        /// <summary>
        /// Verifies that the tokenizer yields results for the quotes and each of the contained words when passed a 
        /// string containing a composite string in quotes.
        /// </summary>
        [TestMethod]
        public void CompositeStringYieldsOneResult()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens("\"Jack be quick\""),
                new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator),
                new QueryToken("Jack", QueryTokenType.Text),
                new QueryToken("be", QueryTokenType.Text),
                new QueryToken("quick", QueryTokenType.Text),
                new QueryToken("\"", QueryTokenType.EndAdjacentTextOperator));
        }

        /// <summary>
        /// Verifies that the tokenizer yields one result when passed a string containing a composite string in quotes
        /// that contains an escaped quote.
        /// </summary>
        [TestMethod]
        public void CompositeStringWithEscapedQuoteYieldsThreeResults()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens(@"""Jack be \""quick\"""""),
                new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator),
                new QueryToken("Jack", QueryTokenType.Text),
                new QueryToken("be", QueryTokenType.Text),
                new QueryToken("\"quick\"", QueryTokenType.Text),
                new QueryToken("\"", QueryTokenType.EndAdjacentTextOperator));
        }

        /// <summary>
        /// Verifies that the tokenizer yields 6 results when passed a string containing two composite strings in quotes
        /// that contains an escaped quote.
        /// </summary>
        [TestMethod]
        public void TwoCompositeStringsYieldsSixResults()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens(@"""First string"" ""Second string"""),
                new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator),
                new QueryToken("First", QueryTokenType.Text),
                new QueryToken("string", QueryTokenType.Text),
                new QueryToken("\"", QueryTokenType.EndAdjacentTextOperator),
                new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator),
                new QueryToken("Second", QueryTokenType.Text),
                new QueryToken("string", QueryTokenType.Text),
                new QueryToken("\"", QueryTokenType.EndAdjacentTextOperator));
        }

        /// <summary>
        /// Verifies that the various operators are parsed correctly from text.
        /// </summary>
        [TestMethod]
        public void OperatorTokensAreParsedCorrectly()
        {
            var target = new QueryTokenizer();
            AssertResults(
                target.ParseQueryTokens(@"& ( | ) ~> >> ~ """),
                new QueryToken("&", QueryTokenType.AndOperator),
                new QueryToken("(", QueryTokenType.OpenBracket),
                new QueryToken("|", QueryTokenType.OrOperator),
                new QueryToken(")", QueryTokenType.CloseBracket),
                new QueryToken("~>", QueryTokenType.PrecedingNearOperator),
                new QueryToken(">>", QueryTokenType.PrecedingOperator),
                new QueryToken("~", QueryTokenType.NearOperator),
                new QueryToken("\"", QueryTokenType.BeginAdjacentTextOperator));
        }

        /// <summary>
        /// Verifies that an unknown operator raises an exception.
        /// </summary>
        [TestMethod]
        public void UnknownOperatorTokensRaiseExceptions()
        {
            var target = new QueryTokenizer();
            AssertRaisesException<QueryParserException>(
                () => target.ParseQueryTokens(@"test1 >>> test2").ToArray(),
                "Unknown query operator: >>>");
        }

        /// <summary>
        /// Asserts the results of a query.
        /// </summary>
        /// <param name="actual">The actual results.</param>
        /// <param name="expected">The expected results.</param>
        private static void AssertResults(IEnumerable<QueryToken> actual, params QueryToken[] expected)
        {
            var actualResults = actual.ToArray();
            Assert.AreEqual(expected.Length, actualResults.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].TokenType, actualResults[i].TokenType);
                Assert.AreEqual(expected[i].TokenText, actualResults[i].TokenText);
            }
        }
    }
}
