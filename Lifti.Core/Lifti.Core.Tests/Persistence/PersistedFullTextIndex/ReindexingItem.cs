// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for item re-indexing in the index.
    /// </summary>
    [TestClass]
    public class ReindexingItem : PersistedFullTextIndexTestBase
    {
        /// <summary>
        /// The item should be removed from and reinserted into the backing store.
        /// </summary>
        [TestMethod]
        public void ShouldRemoveEntryForItem()
        {
            var itemIndexed = false;
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);
            entryManager.Setup(m => m.AddItemIndexEntry(123, "One")).Callback(() => itemIndexed = true);
            entryManager.Setup(m => m.GetIdForItem("One")).Returns(() => itemIndexed ? 123 : 0);
            entryManager.Setup(m => m.RemoveItemEntry(123)).Callback(() => itemIndexed = false);
            entryManager.Setup(m => m.ItemIndexed("One")).Returns(() => itemIndexed);
            entryManager.Setup(m => m.PageManager.AllocateNewItemId()).Returns(123);
            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12, 44);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One", "One");
            index.Index("One", "Once");

            entryManager.Verify(m => m.AddItemIndexEntry(123, "One"), Times.Exactly(2));
            entryManager.Verify(m => m.RemoveItemEntry(123), Times.Exactly(1));
            entryManager.Verify(m => m.RemoveIndexNodeReferenceEntry(9, 12), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(0, 3, 'O'), Times.Exactly(2));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(3, 9, 'N'), Times.Exactly(2));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(9, 12, 'E'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(9, 12, 'C'), Times.Exactly(1));
            entryManager.Verify(m => m.AddIndexNodeReferenceEntry(12, 44, 'E'), Times.Exactly(1));
            entryManager.Verify(m => m.ItemIndexed("One"), Times.Exactly(3));
        }
    }
}
