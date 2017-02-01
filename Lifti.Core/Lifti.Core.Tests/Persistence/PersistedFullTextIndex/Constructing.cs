// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Extensibility;
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the construction of the persisted full text index.
    /// </summary>
    [TestClass]
    public class Constructing
    {
        /// <summary>
        /// Tests the loading of a full text index from an empty file.
        /// </summary>
        [TestMethod]
        public void FromEmptyFile()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.SetupGet(m => m.ItemCount).Returns(0);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.IsNotNull(index.RootNode);
            Assert.AreEqual('\0', index.RootNode.IndexedCharacter);
            Assert.IsTrue(index.RootNode.IsRootNode());
            Assert.AreEqual(0, index.Count);

            entryManager.VerifyAll();
            entryManager.Verify(m => m.Initialize(), Times.Exactly(1));
        }

        /// <summary>
        /// The persisted full text index should report the correct number of items, even though it
        /// is being lazy-loaded.
        /// </summary>
        [TestMethod]
        public void ShouldReportCorrectNumberOfItemsInIndex()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.SetupGet(m => m.ItemCount).Returns(241);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.IsNotNull(index.RootNode);
            Assert.AreEqual('\0', index.RootNode.IndexedCharacter);
            Assert.IsTrue(index.RootNode.IsRootNode());
            Assert.AreEqual(241, index.Count);

            entryManager.VerifyAll();
        }
    }
}
