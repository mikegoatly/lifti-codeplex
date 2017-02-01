// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Extensibility;
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the Count method.
    /// </summary>
    [TestClass]
    public class Count
    {
        /// <summary>
        /// An index should return a count of 0 if it is empty.
        /// </summary>
        [TestMethod]
        public void EmptyIndexShouldReturnZeroCount()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.SetupGet(m => m.ItemCount).Returns(0);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.AreEqual(0, index.Count);

            entryManager.VerifyAll();
        }

        /// <summary>
        /// An index should return the correct count of items it it is populated.
        /// </summary>
        [TestMethod]
        public void PopulatedIndexShouldReturnCorrectCount()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.SetupGet(m => m.ItemCount).Returns(6);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.AreEqual(6, index.Count);

            entryManager.VerifyAll();
        }
    }
}
