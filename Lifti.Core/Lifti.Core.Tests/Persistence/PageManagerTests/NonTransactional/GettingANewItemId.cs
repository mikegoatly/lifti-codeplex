// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for getting new item ids from the page manager.
    /// </summary>
    [TestClass]
    public class GettingANewItemId : PageManagerTestBase
    {
        /// <summary>
        /// The page manager should return a sequentially increasing number when requested.
        /// </summary>
        [TestMethod]
        public void ShouldReturnAnIncrementedNumber()
        {
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 88).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(8, 3, 12))
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            Assert.AreEqual(100, pageManager.AllocateNewItemId());
            Assert.AreEqual(101, pageManager.AllocateNewItemId());
            Assert.AreEqual(102, pageManager.AllocateNewItemId());
        }

        /// <summary>
        /// After obtaining a new number, the backing store should be updated with the next available number.
        /// </summary>
        [TestMethod]
        public void ShouldPersistTheNextNumberValueToTheBackingStore()
        {
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 88).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(8, 3, 12))
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            Assert.AreEqual(100, pageManager.AllocateNewItemId());

            // Verify the backing store has been updated
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(1, 0, 4, 5, 101, 200));
        }
    }
}
