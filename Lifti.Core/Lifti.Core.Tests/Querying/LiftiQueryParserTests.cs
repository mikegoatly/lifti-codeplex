// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Querying
{
    using Lifti.Querying;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the <see cref="LiftiQueryParser"/> class.
    /// </summary>
    [TestFixture]
    public class LiftiQueryParserTests
    {
        /// <summary>
        /// Tests that an empty query results in an empty object model representation.
        /// </summary>
        [Test]
        public void EmptyQueryTextResultsInEmptyQuery()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery(string.Empty, new WordSplitter());

            Assert.AreEqual(string.Empty, query.ToString());
        }

        /// <summary>
        /// Tests that a single word query results in the correct query structure.
        /// </summary>
        [Test]
        public void SingleWordQueryResultsInCorrectObjectModel()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello", new WordSplitter());

            Assert.AreEqual("EXACT(HELLO)", query.ToString());
        }

        /// <summary>
        /// Tests that a single word query with wildcard results in the correct query structure.
        /// </summary>
        [Test]
        public void SingleWordWithWildcardQueryResultsInCorrectObjectModel()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello*", new WordSplitter());

            Assert.AreEqual("LIKE(HELLO)", query.ToString());
        }

        /// <summary>
        /// Tests that a query without joining operators defaults to be connected with ANDs.
        /// </summary>
        [Test]
        public void MultipleWordsWithoutConnectingOperatorsDefaultToAnds()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello*  World! You Rock", new WordSplitter());

            Assert.AreEqual("(LIKE(HELLO) AND ((EXACT(WORLD) AND EXACT(YOU)) AND EXACT(ROCK)))", query.ToString());
        }

        /// <summary>
        /// The and operator is correctly parsed in its own right.
        /// </summary>
        [Test]
        public void AndOperatorCorrectlyParsed()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello* & World!", new WordSplitter());

            Assert.AreEqual("(LIKE(HELLO) AND EXACT(WORLD))", query.ToString());
        }

        /// <summary>
        /// The or operator is correctly parsed in its own right.
        /// </summary>
        [Test]
        public void OrOperatorCorrectlyParsed()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello* | World!", new WordSplitter());

            Assert.AreEqual("(LIKE(HELLO) OR EXACT(WORLD))", query.ToString());
        }

        /// <summary>
        /// When an OR appears before an AND and there are no user placed brackets, the AND should be grouped.
        /// </summary>
        [Test]
        public void WhenAnOrAppearsBeforeAnAndWithoutUserBracketsAndShouldBeGrouped()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello* | World & Blah", new WordSplitter());

            Assert.AreEqual("(LIKE(HELLO) OR (EXACT(WORLD) AND EXACT(BLAH)))", query.ToString());
        }

        /// <summary>
        /// When an AND appears before an OR and there are no user placed brackets, the AND should be grouped.
        /// </summary>
        [Test]
        public void WhenAnAndAppearsBeforeAnOrWithoutUserBracketsAndShouldBeGrouped()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello* & World | Blah", new WordSplitter());

            Assert.AreEqual("((LIKE(HELLO) AND EXACT(WORLD)) OR EXACT(BLAH))", query.ToString());
        }

        /// <summary>
        /// When an OR appears before an AND and there are user placed brackets around the OR, the OR should be grouped.
        /// </summary>
        [Test]
        public void WhenAnOrAppearsBeforeAnAndWithUserBracketsItShouldBeGrouped()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("(Hello* | World) & Blah", new WordSplitter());

            Assert.AreEqual("([(LIKE(HELLO) OR EXACT(WORLD))] AND EXACT(BLAH))", query.ToString());
        }

        /// <summary>
        /// The standard operator precedence should apply even with implicit and statements.
        /// </summary>
        [Test]
        public void OperatorPrecedenceShouldApplyEvenWithImplicitAndStatements()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("A | B C | D", new WordSplitter());

            Assert.AreEqual("(EXACT(A) OR ((EXACT(B) AND EXACT(C)) OR EXACT(D)))", query.ToString());
        }

        /// <summary>
        /// Verifies that the composite text parts are combinable using implicit ands.
        /// </summary>
        [Test]
        public void CompositeTextPartsShouldBeCombinableUsingImplicitAnds()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("\"test A*\" \"next* B\"", new WordSplitter());

            Assert.AreEqual("(\"EXACT(TEST) LIKE(A)\" AND \"LIKE(NEXT) EXACT(B)\")", query.ToString());
        }

        /// <summary>
        /// Verifies that empty composite text parts have no effect on the resulting query.
        /// </summary>
        [Test]
        public void EmptyCompositeTextPartsHaveNoEffectOnQuery()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("\"\" Hello \"  \t\" World\"\"", new WordSplitter());

            Assert.AreEqual("(EXACT(HELLO) AND EXACT(WORLD))", query.ToString());
        }

        /// <summary>
        /// Verifies the edge case where a word splitter splits a single word into multiple words.
        /// </summary>
        [Test]
        public void LikeWordsSplitByPunctuationShouldBeHandledIndependently()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello,World*", new WordSplitter());

            Assert.AreEqual("(LIKE(HELLO) AND LIKE(WORLD))", query.ToString());
        }

        /// <summary>
        /// Verifies the edge case where a word splitter splits a single word into multiple words.
        /// </summary>
        [Test]
        public void WordsSplitByPunctuationShouldBeHandledIndependently()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello,World", new WordSplitter());

            Assert.AreEqual("(EXACT(HELLO) AND EXACT(WORLD))", query.ToString());
        }

        /// <summary>
        /// Binary operators at the start of a query should cause an exception to be raised.
        /// </summary>
        [Test]
        public void BinaryOperatorsAtStartOfQueryShouldRaiseException()
        {
            var parser = new LiftiQueryParser();
            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("  & Hello", new WordSplitter()), 
                "An unexpected operator was encountered: AndOperator");

            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("| Hello", new WordSplitter()),
                "An unexpected operator was encountered: OrOperator");
        }

        /// <summary>
        /// Binary operators at the end of a query without a right part should cause an exception to be raised.
        /// </summary>
        [Test]
        public void BinaryOperatorsAtEndOfQueryShouldRaiseException()
        {
            var parser = new LiftiQueryParser();
            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("Hello & ", new WordSplitter()),
                "The query ended unexpectedly - a token was expected.");

            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("Hello |", new WordSplitter()),
                "The query ended unexpectedly - a token was expected.");
        }

        /// <summary>
        /// Verifies that user placed brackets at the root of the query are honoured.
        /// </summary>
        [Test]
        public void BracketedStatementsAtRootShouldBeHonoured()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("(Hello World)", new WordSplitter());

            Assert.AreEqual("[(EXACT(HELLO) AND EXACT(WORLD))]", query.ToString());
        }

        /// <summary>
        /// Verifies that NEAR statements are higher precedence than other operators.
        /// </summary>
        [Test]
        public void NearStatementsShouldBeHigherPrecedenceThanOtherOperators()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello ~ World | Hello ~ World", new WordSplitter());

            Assert.AreEqual("((EXACT(HELLO) NEAR EXACT(WORLD)) OR (EXACT(HELLO) NEAR EXACT(WORLD)))", query.ToString());
        }

        /// <summary>
        /// Verifies that PRECEDING NEAR statements are higher precedence than other operators.
        /// </summary>
        [Test]
        public void PrecedingNearStatementsShouldBeHigherPrecedenceThanOtherOperators()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello ~> World | Hello ~> World", new WordSplitter());

            Assert.AreEqual("((EXACT(HELLO) PRECEDING NEAR EXACT(WORLD)) OR (EXACT(HELLO) PRECEDING NEAR EXACT(WORLD)))", query.ToString());
        }

        /// <summary>
        /// Verifies that PRECEDING statements are higher precedence than other operators.
        /// </summary>
        [Test]
        public void PrecedingStatementsShouldBeHigherPrecedenceThanOtherOperators()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello >> World | Hello >> World", new WordSplitter());

            Assert.AreEqual("((EXACT(HELLO) PRECEDING EXACT(WORLD)) OR (EXACT(HELLO) PRECEDING EXACT(WORLD)))", query.ToString());
        }

        /// <summary>
        /// Verifies that brackets affect the precedence of NEAR statements.
        /// </summary>
        [Test]
        public void BracketsShouldAffectPrecedenceOfNearStatements()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("Hello ~ (World | Hello) ~ World", new WordSplitter());

            Assert.AreEqual("(EXACT(HELLO) NEAR ([(EXACT(WORLD) OR EXACT(HELLO))] NEAR EXACT(WORLD)))", query.ToString());
        }

        /// <summary>
        /// Verifies that user placed brackets at the root of the query are honoured.
        /// </summary>
        [Test]
        public void EmptyBracketsShouldHaveNoEffectOnQuery()
        {
            var parser = new LiftiQueryParser();
            var query = parser.ParseQuery("() Hello (  )", new WordSplitter());

            Assert.AreEqual("EXACT(HELLO)", query.ToString());
        }

        /// <summary>
        /// Verifies that if a close bracket is encountered before an open bracket, an
        /// exception is raised.
        /// </summary>
        [Test]
        public void UnexpectedCloseBracketsShouldRaiseException()
        {
            var parser = new LiftiQueryParser();
            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("() Hello  )", new WordSplitter()),
                "Unexpected token encountered: CloseBracket");
        }

        /// <summary>
        /// Verifies that if a close bracket is not encountered before the end of a query after an open
        /// bracket is encountered, an exception is raised.
        /// </summary>
        [Test]
        public void UnclosedBracketsShouldRaiseException()
        {
            var parser = new LiftiQueryParser();
            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("( Hello  ", new WordSplitter()),
                "Token expected: CloseBracket");
        }

        /// <summary>
        /// Verifies that if a close quote is not encountered before the end of a query after an open
        /// quote is encountered, an exception is raised.
        /// </summary>
        [Test]
        public void UnclosedQuotesShouldRaiseException()
        {
            var parser = new LiftiQueryParser();
            Assert.Throws<QueryParserException>(
                () => parser.ParseQuery("\" Hello  ", new WordSplitter()),
                "Token expected: EndAdjacentTextOperator");
        }
    }
}
