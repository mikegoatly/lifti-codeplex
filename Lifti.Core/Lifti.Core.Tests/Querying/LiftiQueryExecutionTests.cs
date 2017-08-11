// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Querying
{
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Querying;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the execution of complex LIFTI queries in conjunction with a full text index.
    /// </summary>
    [TestFixture]
    public class LiftiQueryExecutionTests
    {
        /// <summary>
        /// The indexer used in the tests.
        /// </summary>
        private FullTextIndex<Customer> indexer = CreateFullTextIndexer();

        /// <summary>
        /// Executing a query with a null root should return no items.
        /// </summary>
        [Test]
        public void ExecutingQueryWithNullRootShouldReturnNoItems()
        {
            var query = new FullTextQuery();
            AssertSearchResults(this.indexer.Search(query));
        }

        /// <summary>
        /// Tests exact word matches don't yield like words, even if there are some.
        /// </summary>
        [Test]
        public void ExactWordMatchShouldOnlyYieldExactWordsEvenIfThereAreLikeWords()
        {
            AssertSearchResults(this.indexer.Search("ear"), "F");
            AssertSearchResults(this.indexer.Search("e"), "A");
        }

        /// <summary>
        /// Tests like word matches.
        /// </summary>
        [Test]
        public void LikeWordMatchShouldYieldLikeWords()
        {
            AssertSearchResults(this.indexer.Search("ear*"), "E", "F");
        }

        /// <summary>
        /// Tests that AND operators restrict results.
        /// </summary>
        [Test]
        public void AndOperatorsShouldRestrictResults()
        {
            AssertSearchResults(this.indexer.Search("E* & C* & M*"), "A", "F");
        }

        /// <summary>
        /// Tests that AND operators where the right search returns nothing returns nothing overall.
        /// </summary>
        [Test]
        public void AndOperatorsShouldReturnNoResultsIfRightPartReturnsNothing()
        {
            AssertSearchResults(this.indexer.Search("E* & chee"));
        }

        /// <summary>
        /// Tests that OR operators expand results.
        /// </summary>
        [Test]
        public void OrOperatorsShouldExpandResults()
        {
            AssertSearchResults(this.indexer.Search("E | M*"), "A", "D", "F", "G");
        }

        /// <summary>
        /// Tests that bracketed AND and OR operators that are constructed opposed to
        /// default operator precedent provide the correct results.
        /// </summary>
        [Test]
        public void BracketedAndOrOperationsShouldReturnResults()
        {
            // First verify that the default operator precedence works fine
            AssertSearchResults(this.indexer.Search("E* | H* & Cheese"), "A", "E", "F", "G");
            AssertSearchResults(this.indexer.Search("E* | (H* & Cheese)"), "A", "E", "F", "G");

            // Now verify the manually placed operators work fine
            AssertSearchResults(this.indexer.Search("(E* | H*) & Cheese"), "F", "G");
        }

        /// <summary>
        /// Tests that sequential text operations only work where text is sequential, but not if
        /// the words appear in the right order spread out through the text.
        /// </summary>
        [Test]
        public void SequentialTextOperationsShouldOnlyWorkWhereTextIsSequential()
        {
            // This will not return customer E because X Y and Z are split up with a B
            AssertSearchResults(this.indexer.Search("\"X Y Z\""), "A", "D");
        }

        /// <summary>
        /// Tests that near operator works in its own right.
        /// </summary>
        [Test]
        public void NearOperatorShouldReturnResults()
        {
            AssertSearchResults(this.indexer.Search("B ~ I"), "B", "C");
        }

        /// <summary>
        /// Tests that multiple near operators are combined correctly.
        /// </summary>
        [Test]
        public void MultipleNearOperatorsShouldBeRestrictive()
        {
            AssertSearchResults(this.indexer.Search("B ~ Y ~ Z"), "C", "E");
            AssertSearchResults(this.indexer.Search("B ~ Y ~ X"), "E");

            // This will exclude A because I is too far away from C
            AssertSearchResults(this.indexer.Search("B ~ C ~ I"), "B");
        }

        /// <summary>
        /// Tests that a NEAR statement can be combined with an OR statement.
        /// </summary>
        [Test]
        public void NearOperatorShouldWorkWithOrStatements()
        {
            // E will be excluded because it doesn't contain an I or a J
            // A will be excluded because B isn't near I or J
            AssertSearchResults(this.indexer.Search("B ~ (I | J)"), "B", "C");
        }

        /// <summary>
        /// Tests that a simple preceding statement works.
        /// </summary>
        [Test]
        public void PrecedingOperatorShouldWork()
        {
            AssertSearchResults(this.indexer.Search("P >> M"), "D");
        }

        /// <summary>
        /// Tests that the preceding operator works in conjunction with other operators.
        /// </summary>
        [Test]
        public void PrecedingOperatorShouldWorkInConjunctionWithOtherOperators()
        {
            AssertSearchResults(this.indexer.Search("B >> Z & I"), "A", "C");
            AssertSearchResults(this.indexer.Search("(B >> Z) & (B >> I)"), "A");
        }

        /// <summary>
        /// Asserts the search results are as expected.
        /// </summary>
        /// <param name="results">The actual results.</param>
        /// <param name="customers">The expected customer names.</param>
        private static void AssertSearchResults(IEnumerable<Customer> results, params string[] customers)
        {
            var resultsArray = results.ToArray();
            Assert.AreEqual(customers.Length, resultsArray.Length);

            Assert.IsTrue(resultsArray.All(c => customers.Contains(c.Name)));
        }

        /// <summary>
        /// Creates the sample full text indexer.
        /// </summary>
        /// <returns>The constructed full text indexer.</returns>
        private static FullTextIndex<Customer> CreateFullTextIndexer()
        {
            var index = new FullTextIndex<Customer>
                        {
                QueryParser = new LiftiQueryParser()
            };

            var customers = new[]
                        {
                            new Customer { Name = "A", Biography = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z" },
                            new Customer { Name = "B", Biography = "B C I J" },
                            new Customer { Name = "C", Biography = "I B Y Z" },
                            new Customer { Name = "D", Biography = "P X Y X Y Z N M" },
                            new Customer { Name = "E", Biography = "X Y B Z Early" },
                            new Customer { Name = "F", Biography = "Ear Move Cheese Plant" },
                            new Customer { Name = "G", Biography = "Hello Cheese Move" }
                        };

            index.Index(customers, c => c.Biography);

            return index;
        }
    }
}
