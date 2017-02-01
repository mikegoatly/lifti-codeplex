// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.UpdatableFullTextIndexTests
{
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for re-indexing items in an <see cref="UpdatableFullTextIndex{TKey}"/>.
    /// </summary>
    [TestClass]
    public class Reindexing
    {
        /// <summary>
        /// Tests that re-indexing an item after its text has changed should remove only the 
        /// old words from the index, and add in the new ones.
        /// </summary>
        [TestMethod]
        public void ReindexingAnItemAfterTextChangedShouldRemoveOnlyTheOldWordsFromTheIndex()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Test" };
            var item2 = new Customer { Name = "Test2", Biography = "Test" };

            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(new[] { item1, item2 }, i => i.Biography);

            Assert.AreEqual(2, index.Search("Test").Count());
            Assert.AreEqual(2, index.Count);

            item2.Biography = "Blah";
            index.Index(item2, item2.Biography);

            Assert.AreEqual("Test1", index.Search("Test").Single().Name);
            Assert.AreEqual("Test2", index.Search("Blah").Single().Name);
            Assert.AreEqual(2, index.Count);
        }

        /// <summary>
        /// Tests that re-indexing all items after their text has changed should effectively
        /// rebuild the index.
        /// </summary>
        [TestMethod]
        public void ReindexingAllItemAfterTextChangedShouldRebuildIndex()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Testing" };
            var item2 = new Customer { Name = "Test2", Biography = "Test" };

            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(new[] { item1, item2 }, i => i.Biography);

            Assert.AreEqual(2, index.Search("Test").Count());
            Assert.AreEqual(1, index.Search("Testing").Count());
            Assert.AreEqual(2, index.Count);

            item1.Biography = "Bob";
            item2.Biography = "Blah";
            index.Index(new[] { item1, item2 }, i => i.Biography);

            Assert.AreEqual("Test1", index.Search("Bob").Single().Name);
            Assert.AreEqual("Test2", index.Search("Blah").Single().Name);
            Assert.AreEqual(0, index.Search("Test").Count());
            Assert.AreEqual(0, index.Search("Testing").Count());
            Assert.AreEqual(2, index.Count);
        }
    }
}
