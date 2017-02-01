// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Extensibility;
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the Contains method.
    /// </summary>
    [TestClass]
    public class Contains
    {
        /// <summary>
        /// The Contains method should return false if the item doesn't exist in the index.
        /// </summary>
        [TestMethod]
        public void CheckingForAnItemNotInTheIndexShouldReturnFalse()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.Setup(m => m.ItemIndexed(999)).Returns(false);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.IsFalse(index.Contains(999));

            entryManager.VerifyAll();
        }

        /// <summary>
        /// The Contains method should return true if the item exists in the index.
        /// </summary>
        [TestMethod]
        public void CheckingForAnItemInTheIndexShouldReturnTrue()
        {
            var entryManager = new Mock<IPersistedEntryManager<int>>(MockBehavior.Strict);
            entryManager.Setup(m => m.Initialize());
            entryManager.Setup(m => m.ItemIndexed(999)).Returns(true);
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var index = new PersistedFullTextIndex<int>(entryManager.Object, extensibilityService.Object);

            Assert.IsTrue(index.Contains(999));

            entryManager.VerifyAll();
        }
    }
}
