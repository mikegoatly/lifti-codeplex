// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for indexing items in the persisted full text index.
    /// </summary>
    [TestFixture]
    public class IndexingItem : PersistedFullTextIndexTestBase
    {
        /// <summary>
        /// Indexing a null list of items should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingNullListOfItemsShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index(null, c => c), "itemKeys");
        }

        /// <summary>
        /// Indexing an item against null text should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingWithNullTextShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index("test", (string)null), "text");
        }

        /// <summary>
        /// Indexing an item against a null text reader should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingItemWithNullTextReaderShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index(new Customer(), c => c.Name, null), "readText");
        }

        /// <summary>
        /// Indexing an item against a null key reader should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingItemWithNullKeyReaderShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index(new Customer(), null, c => c.Biography), "readKey");
        }

        /// <summary>
        /// Indexing a list of items against a null text reader should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingItemsWithNullTextReaderShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index(new Customer[0], c => c.Name, null), "readText");
        }

        /// <summary>
        /// Indexing a list of items against a null key reader should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingItemsWithNullKeyReaderShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index(new Customer[0], null, c => c.Biography), "readKey");
        }

        /// <summary>
        /// Indexing a list of items against a null key reader should raise an argument null exception.
        /// </summary>
        [Test]
        public void IndexingItemsWithNullItemListShouldRaiseException()
        {
            var index = CreatePersistedFullTextIndex();
            this.AssertRaisesArgumentNullException(() => index.Index((IEnumerable<Customer>)null, c => c.Name, c => c.Biography), "items");
        }

        /// <summary>
        /// A new entry should be added for the indexed item.
        /// </summary>
        [Test]
        public void ShouldAddEntryForIndexedItem()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.ItemIndexed("Value One")).Returns(false);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("Value One", "Value One");

            entryManager.Verify(m => m.PageManager.AllocateNewItemId(), Times.Exactly(1));
            entryManager.Verify(m => m.AddItemIndexEntry(123, "Value One"), Times.Exactly(1));
        }

        /// <summary>
        /// A new entry should be added each time a new node is created.
        /// </summary>
        [Test]
        public void ShouldAddEntryForEachNodeCreatedOnFirstUse()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12);
            entryManager.Setup(m => m.ItemIndexed("One")).Returns(false);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One", "One");

            entryManager.Verify(m => m.PageManager.AllocateNewIndexNodeId(), Times.Exactly(3));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(0, 3, 'O'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(3, 9, 'N'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(9, 12, 'E'), Times.Exactly(1));
        }

        /// <summary>
        /// Existing node entries should be re-used when indexing subsequent words.
        /// </summary>
        [Test]
        public void NodeEntriesShouldBeReusedOnSubsequentIndexing()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12, 40, 42);
            entryManager.Setup(m => m.ItemIndexed("One Once")).Returns(false);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One Once", "One Once");

            entryManager.Verify(m => m.PageManager.AllocateNewIndexNodeId(), Times.Exactly(5));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(0, 3, 'O'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(3, 9, 'N'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(9, 12, 'E'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(9, 40, 'C'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(40, 42, 'E'), Times.Exactly(1));
        }

        /// <summary>
        /// Item references should be added for each indexed word.
        /// </summary>
        [Test]
        public void ItemReferenceShouldBeAddedForEachIndexedWord()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12, 40, 42);
            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.GetIdForItem("One Once One")).Returns(123);
            entryManager.Setup(m => m.ItemIndexed("One Once One")).Returns(false);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One Once One", "One Once One");

            entryManager.Verify(m => m.PageManager.AllocateNewItemId(), Times.Exactly(1));
            entryManager.Verify(m => m.PageManager.AllocateNewIndexNodeId(), Times.Exactly(5));
            entryManager.Verify(m => m.AddNodeItemEntry(12, 123, 0), Times.Exactly(1));
            entryManager.Verify(m => m.AddNodeItemEntry(12, 123, 2), Times.Exactly(1));
            entryManager.Verify(m => m.AddNodeItemEntry(42, 123, 1), Times.Exactly(1));
        }

        /// <summary>
        /// When an item is re-indexed and nodes are initially removed, the same ids should
        /// be re-used.
        /// </summary>
        [Test]
        public void ReindexingAnItemShouldReuseNodeIds()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            var ids = new[] { 3, 9, 12, 40, 42 };
            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(ids);
            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.ItemIndexed("One Once One")).ReturnsInOrder(false, true, true);
            entryManager.Setup(m => m.GetIdForItem("One Once One")).Returns(123);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One Once One", "One Once One");
            index.Index("One Once One", "One Once One");

            entryManager.Verify(m => m.PageManager.AllocateNewItemId(), Times.Exactly(1));
            entryManager.Verify(m => m.PageManager.AllocateNewIndexNodeId(), Times.Exactly(5));
            entryManager.Verify(m => m.AddItemIndexEntry(123, "One Once One"), Times.Exactly(2));
            entryManager.Verify(m => m.AddNodeItemEntry(It.Is<int>(i => ids.Contains(i)), 123, 0), Times.Exactly(2));
            entryManager.Verify(m => m.AddNodeItemEntry(It.Is<int>(i => ids.Contains(i)), 123, 1), Times.Exactly(2));
            entryManager.Verify(m => m.AddNodeItemEntry(It.Is<int>(i => ids.Contains(i)), 123, 2), Times.Exactly(2));
        }
    }
}
