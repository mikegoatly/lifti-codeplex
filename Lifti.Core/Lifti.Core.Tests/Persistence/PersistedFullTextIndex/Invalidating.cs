namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using System.Linq;

    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the node invalidation process.
    /// </summary>
    [TestFixture]
    public class Invalidating : PersistedFullTextIndexTestBase
    {
        /// <summary>
        /// When a node is invalidated, its child information should be re-loaded when required.
        /// </summary>
        [Test]
        public void InvalidatingNodeShouldCauseIndexToBeReloaded()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);

            entryManager.Setup(m => m.PageManager.AllocateNewIndexNodeId()).ReturnsInOrder(3, 9, 12);

            var index = CreatePersistedFullTextIndex(entryManager);

            index.Index("One", "One");

            entryManager.Verify(m => m.GetIndexNodeEntries(0), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(3), Times.Never());
            entryManager.Verify(m => m.GetIndexNodeEntries(9), Times.Never());

            entryManager.Setup(m => m.GetIndexNodeEntries(0)).Returns(new[] { new NodeReferenceIndexNodeEntry(0, 3, 'O') });
            entryManager.Setup(m => m.GetIndexNodeEntries(3)).Returns(new[] { new NodeReferenceIndexNodeEntry(3, 9, 'N') });
            entryManager.Setup(m => m.GetIndexNodeEntries(9)).Returns(new[] { new NodeReferenceIndexNodeEntry(9, 12, 'E') });

            // Clear the node 
            ((PersistedIndexNode<string>)index.RootNode).Clear();

            // Navigate the nodes - this will cause it to reload its child entries
            index.RootNode.GetDirectAndChildItems().ToArray();

            entryManager.Verify(m => m.PageManager.AllocateNewIndexNodeId(), Times.Exactly(3));
            entryManager.Verify(m => m.GetIndexNodeEntries(0), Times.Exactly(2));
            entryManager.Verify(m => m.GetIndexNodeEntries(3), Times.Exactly(1));
            entryManager.Verify(m => m.GetIndexNodeEntries(9), Times.Exactly(1));
        }
    }
}
