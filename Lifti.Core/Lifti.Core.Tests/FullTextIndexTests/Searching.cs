// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the searching of the <see cref="FullTextIndex{TKey}"/>.
    /// </summary>
    [TestClass]
    public class Searching : FullTextIndexTestBase
    {
        /// <summary>
        /// Tests that it's possible to index and search for a word with.an umlaut in.
        /// </summary>
        [TestMethod]
        public void ShouldBePossibleToSearchForItemWithUmlaut()
        {
            using (var index = new FullTextIndex<string>())
            {
                index.Index("Test", "gerät blah");
                Assert.IsTrue(index.Search("gerät").Count() == 1);
            }
        }

        /// <summary>
        /// Tests the case where the full text index is empty.
        /// </summary>
        [TestMethod]
        public void EmptyIndexSearch()
        {
            var indexer = CreateCustomerFullTextIndexer();
            Assert.AreEqual(0, indexer.Search("System Object").Count());
        }

        /// <summary>
        /// Tests the matching of one item with one search word.
        /// </summary>
        [TestMethod]
        public void TestMatchingOneItemOneSearchWord()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Biscuit and co" };
            var customer2 = new Customer { Name = "Lemon cakes" };

            indexer.Index(customer1, customer1.Name);
            indexer.Index(customer2, customer2.Name);

            var results = indexer.Search("Biscuit").ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(customer1, results[0]);
        }

        /// <summary>
        /// Tests the indexer with multiple search words.
        /// </summary>
        [TestMethod]
        public void TestMultipleSearchWords()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Better than biscuits fireworks-mix" };
            var customer2 = new Customer { Name = "Biscuit mixer" };
            var customer3 = new Customer { Name = "Slightly soggy biscuits & co" };

            indexer.Index(new[] { customer1, customer2, customer3 }, c => c.Name);

            var results = indexer.Search("mix biscuit").ToArray();
            Assert.AreEqual(2, results.Length);
            Assert.AreSame(customer1, results[0]);
            Assert.AreSame(customer2, results[1]);

            // Note that customer 3 is not returned because they don't have the word Mix in their name.
        }

        /// <summary>
        /// Tests the matching of a word that's cased differently from the indexed word.
        /// </summary>
        [TestMethod]
        public void TestCaseInsensitivityMatching()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Bananas r us" };

            indexer.Index(customer1, customer1.Name);

            var results = indexer.Search("Bananas").ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(customer1, results[0]);
        }

        /// <summary>
        /// Tests the case where an apostrophe is searched for, or in the original text.
        /// </summary>
        [TestMethod]
        public void TestApostropheMatching()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Daves Emporium" };
            var customer2 = new Customer { Name = "Dave's Emporium" };

            indexer.Index(customer1, customer1.Name);
            indexer.Index(customer2, customer2.Name);

            var results = indexer.Search("Daves").ToArray();
            Assert.AreEqual(2, results.Length);
            Assert.AreSame(customer1, results[0]);
            Assert.AreSame(customer2, results[1]);
        }

        /// <summary>
        /// Tests the case where the search words are only part of the words in the indexed text.
        /// </summary>
        [TestMethod]
        public void TestPartialMatching()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Power Pollen Cleaners" };
            var customer2 = new Customer { Name = "Roger's Policing Parrots" };

            indexer.Index(customer1, customer1.Name);
            indexer.Index(customer2, customer2.Name);

            var results = indexer.Search("pol").ToArray();
            Assert.AreEqual(2, results.Length);
            Assert.AreSame(customer1, results[0]);
            Assert.AreSame(customer2, results[1]);
        }

        /// <summary>
        /// Tests searching with empty text.
        /// </summary>
        [TestMethod]
        public void TestSearchingOnEmptyText()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Power Pollen Cleaners" };
            var customer2 = new Customer { Name = "Roger's Policing Parrots" };

            indexer.Index(customer1, customer1.Name);
            indexer.Index(customer2, customer2.Name);

            var results = indexer.Search(string.Empty).ToArray();
            Assert.AreEqual(0, results.Length);
        }

        /// <summary>
        /// Tests searching on text whose words are separated with punctuation.
        /// </summary>
        [TestMethod]
        public void TestPunctuationWordBreaks()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Amazing.Fantastic-Fireworks" };

            indexer.Index(customer1, customer1.Name);

            var results = indexer.Search("Fantastic").ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(customer1, results[0]);
        }

        /// <summary>
        /// Tests that symbols don't affect search words.
        /// </summary>
        [TestMethod]
        public void TestSymbolHandling()
        {
            var indexer = CreateCustomerFullTextIndexer();
            var customer1 = new Customer { Name = "Cheese and stuff" };

            indexer.Index(customer1, customer1.Name);

            var results = indexer.Search("<Cheese>").ToArray();
            Assert.AreEqual(1, results.Length);
            Assert.AreSame(customer1, results[0]);
        }
    }
}
