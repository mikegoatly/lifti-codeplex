// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for item removal from the index.
    /// </summary>
    [TestFixture]
    public class RemovingItem : PersistedFullTextIndexTestBase
    {
        /// <summary>
        /// The item should be removed from the backing store.
        /// </summary>
        [Test]
        public void ShouldRemoveEntryForItem()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.GetIdForItem("Value One")).Returns(123);
            entryManager.Setup(m => m.ItemIndexed("Value One")).ReturnsInOrder(false, true);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("Value One", "Value One");
            index.Remove("Value One");

            entryManager.Verify(m => m.AddItemIndexEntry(123, "Value One"), Times.Exactly(1));
            entryManager.Verify(m => m.RemoveItemEntry(123), Times.Exactly(1));
            entryManager.Verify(m => m.ItemIndexed("Value One"), Times.Exactly(2));
        }

        /// <summary>
        /// Nodes should be removed from the backing store when they are no longer used.
        /// </summary>
        [Test]
        public void ShouldRemoveEntryForEachNodeNoLongerUsed()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).ReturnsInOrder(123, 222);
            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12, 44, 67);
            entryManager.Setup(m => m.GetIdForItem("One")).Returns(123);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One", "One");
            entryManager.Setup(m => m.ItemIndexed("One")).Returns(true);
            index.Index("Only", "Only");
            entryManager.Setup(m => m.ItemIndexed("Only")).Returns(true);
            index.Remove("One");

            entryManager.Verify(m => m.RemoveNodeItemEntry(12, 123), Times.Exactly(1));
            entryManager.Verify(m => m.RemoveIndexNodeReferenceEntry(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(1));
        }

        /// <summary>
        /// Tests that removing an item that has not been indexed does not cause any exceptions to
        /// be thrown and leaves the index unchanged.
        /// </summary>
        [Test]
        public void RemovingUnknownItemShouldNotRaiseException()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.ItemIndexed("Value Two")).Returns(false);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("Value One", "Value One");
            entryManager.Setup(m => m.ItemIndexed("Value One")).Returns(true);
            index.Remove("Value Two");

            entryManager.Verify(m => m.RemoveNodeItemEntry(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }
    }
}
