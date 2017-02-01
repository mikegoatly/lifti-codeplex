// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the indexing of items from the <see cref="FullTextIndex{TKey}"/>.
    /// </summary>
    [TestClass]
    public class Indexing : FullTextIndexTestBase
    {
        /// <summary>
        /// Nodes created as part of the indexing process should not be identified as root nodes.
        /// </summary>
        [TestMethod]
        public void NonRootIndexNodesShouldNotBeIdentifiedAsRootNodes()
        {
            var index = new FullTextIndex<string>();
            index.Index("test", "test");

            var node = index.RootNode;
            for (var i = 0; i < 4; i++)
            {
                node = node.GetChildNodes().FirstOrDefault();
                Assert.IsNotNull(node);
                Assert.IsFalse(node.IsRootNode());
            }
        }

        /// <summary>
        /// Indexing a null list of items should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingNullListOfItemsShouldRaiseException()
        {
            var index = new FullTextIndex<Customer>();
            AssertRaisesArgumentNullException(() => index.Index(null, c => c.Biography), "itemKeys");
        }

        /// <summary>
        /// Indexing an item against null text should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingWithNullTextShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index("test", (string)null), "text");
        }

        /// <summary>
        /// Indexing an item against a null text reader should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingItemWithNullTextReaderShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index(new Customer(), c => c.Name, null), "readText");
        }

        /// <summary>
        /// Indexing an item against a null key reader should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingItemWithNullKeyReaderShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index(new Customer(), null, c => c.Biography), "readKey");
        }

        /// <summary>
        /// Indexing a list of items against a null text reader should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingItemsWithNullTextReaderShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index(new Customer[0], c => c.Name, null), "readText");
        }

        /// <summary>
        /// Indexing a list of items against a null key reader should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingItemsWithNullKeyReaderShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index(new Customer[0], null, c => c.Biography), "readKey");
        }

        /// <summary>
        /// Indexing a list of items against a null key reader should raise an argument null exception.
        /// </summary>
        [TestMethod]
        public void IndexingItemsWithNullItemListShouldRaiseException()
        {
            var index = new FullTextIndex<string>();
            AssertRaisesArgumentNullException(() => index.Index((IEnumerable<Customer>)null, c => c.Name, c => c.Biography), "items");
        }

        /// <summary>
        /// Indexing a list of items should cause all provided items to be stored in the index.
        /// </summary>
        [TestMethod]
        public void IndexingListOfArbitraryItems()
        {
            var index = new FullTextIndex<string>();
            var customer1 = new Customer { Name = "A", Biography = "Best customer" };
            var customer2 = new Customer { Name = "B", Biography = "Worst customer" };
            index.Index(new[] { customer1, customer2 }, c => c.Name, c => c.Biography);

            var results = index.Search("customer").ToArray();
            Assert.IsTrue((new[] { "A", "B" }).All(i => results.Contains(i)));
        }

        /// <summary>
        /// Indexing a single item should cause it to be stored in the index.
        /// </summary>
        [TestMethod]
        public void IndexingSingleArbitraryItem()
        {
            var index = new FullTextIndex<string>();
            var customer1 = new Customer { Name = "A", Biography = "Best customer" };
            var customer2 = new Customer { Name = "B", Biography = "Worst customer" };
            index.Index(customer1, c => c.Name, c => c.Biography);
            index.Index(customer2, c => c.Name, c => c.Biography);

            var results = index.Search("customer").ToArray();
            Assert.IsTrue((new[] { "A", "B" }).All(i => results.Contains(i)));
        }

        /// <summary>
        /// Indexing multiple keys should cause each to be stored in the index.
        /// </summary>
        [TestMethod]
        public void IndexingMultipleKeys()
        {
            var index = new FullTextIndex<string>();

            index.Index(new[] { "A", "B" }, s => s == "A" ? "Best customer" : "Worst customer");

            var results = index.Search("customer").ToArray();
            Assert.IsTrue((new[] { "A", "B" }).All(i => results.Contains(i)));

            results = index.Search("worst").ToArray();
            Assert.IsTrue((new[] { "B" }).All(i => results.Contains(i)));
        }

        /// <summary>
        /// Indexing a single key should cause it to be stored in the index.
        /// </summary>
        [TestMethod]
        public void IndexingSingleKey()
        {
            var index = new FullTextIndex<string>();

            index.Index("A", "Best customer");
            index.Index("B", "Worst customer");

            var results = index.Search("customer").ToArray();
            Assert.IsTrue((new[] { "A", "B" }).All(i => results.Contains(i)));
        }
    }
}
