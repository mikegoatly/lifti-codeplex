// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.UpdatableFullTextIndexTests
{
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Tests for removing items from an <see cref="UpdatableFullTextIndex{TKey}"/>.
    /// </summary>
    [TestFixture]
    public class Removing
    {
        /// <summary>
        /// Removing a null list of items should raise an argument null exception.
        /// </summary>
        [Test]
        public void RemovingNullListOfItemsShouldRaiseException()
        {
            var index = new UpdatableFullTextIndex<Customer>();
            this.AssertRaisesArgumentNullException(() => index.Remove((IEnumerable<Customer>)null), "itemKeys");
        }

        /// <summary>
        /// Tests that removing the only item in the index results in empty root node.
        /// </summary>
        [Test]
        public void RemovingTheOnlyItemInTheIndexShouldResultInEmptyIndex()
        {
            var item = new Customer { Name = "Test", Biography = "Test Testing" };
            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(item, item.Biography);

            Assert.AreEqual(1, index.Search("Test").Count());
            Assert.AreEqual(1, index.Search("Testing").Count());
            Assert.AreEqual(1, index.Count);

            index.Remove(item);

            Assert.AreEqual(0, index.Search("Test").Count());
            Assert.AreEqual(0, index.Search("Testing").Count());
            Assert.AreEqual(0, index.Count);
        }

        /// <summary>
        /// Tests that removing the only items in the index results in empty root node.
        /// </summary>
        [Test]
        public void RemovingTheOnlyItemsInTheIndexShouldResultInEmptyIndex()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Test Testing" };
            var item2 = new Customer { Name = "Test2", Biography = "Another test" };
            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(item1, item1.Biography);
            index.Index(item2, item2.Biography);

            Assert.AreEqual(2, index.Search("Test").Count());
            Assert.AreEqual(1, index.Search("Testing").Count());
            Assert.AreEqual(1, index.Search("Another").Count());
            Assert.AreEqual(2, index.Count);

            index.Remove(new[] { item1, item2 });

            Assert.AreEqual(0, index.Search("Test").Count());
            Assert.AreEqual(0, index.Search("Testing").Count());
            Assert.AreEqual(0, index.Search("Another").Count());
            Assert.AreEqual(0, index.Count);
        }

        /// <summary>
        /// Tests that indexing two items with the same words and removing one of them leaves the index
        /// still able to return the first.
        /// </summary>
        [Test]
        public void IndexingTwoItemsWithSameWordsAndRemovingOneShouldNotAffectTheOther()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Test Testing" };
            var item2 = new Customer { Name = "Test2", Biography = "Test Testing" };

            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(item1, item1.Biography);
            index.Index(item2, item2.Biography);

            Assert.AreEqual(2, index.Search("Test").Count());
            Assert.AreEqual(2, index.Search("Testing").Count());
            Assert.AreEqual(2, index.Count);

            index.Remove(item1);

            Assert.AreEqual("Test2", index.Search("Test").Single().Name);
            Assert.AreEqual("Test2", index.Search("Testing").Single().Name);
            Assert.AreEqual(1, index.Count);
        }

        /// <summary>
        /// Tests that removing a longer word from the index doesn't affect any shorter
        /// versions of the word.
        /// </summary>
        [Test]
        public void RemovingWordThatIsLongerThanAnotherWordShouldNotAffectTheShorter()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Testing" };
            var item2 = new Customer { Name = "Test2", Biography = "Test" };

            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(item1, item1.Biography);
            index.Index(item2, item2.Biography);

            Assert.AreEqual(2, index.Search("Test").Count());
            Assert.AreEqual(1, index.Search("Testing").Count());
            Assert.AreEqual(2, index.Count);

            index.Remove(item1);

            Assert.AreEqual("Test2", index.Search("Test").Single().Name);
            Assert.AreEqual(0, index.Search("Testing").Count());
            Assert.AreEqual(1, index.Count);
        }

        /// <summary>
        /// Tests that removing an item that has not been indexed does not cause any exceptions to
        /// be thrown and leaves the index unchanged.
        /// </summary>
        [Test]
        public void RemovingUnknownItemShouldNotRaiseException()
        {
            var item1 = new Customer { Name = "Test1", Biography = "Test" };
            var item2 = new Customer { Name = "Test2", Biography = "Test" };

            var index = new UpdatableFullTextIndex<Customer>();

            index.Index(item1, item1.Biography);

            Assert.AreEqual(1, index.Search("Test").Count());
            Assert.AreEqual(1, index.Count);

            index.Remove(item2);

            Assert.AreEqual(1, index.Search("Test").Count());
            Assert.AreEqual(1, index.Count);
        }
    }
}
