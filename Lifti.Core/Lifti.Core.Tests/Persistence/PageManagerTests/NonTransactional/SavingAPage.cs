// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the Save method of the page manager.
    /// </summary>
    [TestClass]
    public class SavingAPage : PageManagerTestBase
    {
        /// <summary>
        /// Saving a populated index node page should call through to the IO manager, saving data in the expected format.
        /// </summary>
        [TestMethod]
        public void SavingAPopulatedIndexNodePageShouldCallThroughToIOManager()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 88).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(8, 3, 12))
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = (IndexNodeDataPage)pageManager.GetPage(0);
            page.AddEntry(new ItemReferenceIndexNodeEntry(7, 9, 12));
            page.AddEntry(new NodeReferenceIndexNodeEntry(7, 8, 'x'));
            pageManager.SavePage(page);

            VerifyPersistedData(
                ioManager.GetPageData(0),
                Data.IndexNodePage(null, 2, 4, 2, 7, Data.PageHeaderSize + 13 + 10 + 13 + 10).ItemReference(2, 5, 88).NodeReference(6, 4, 'a').ItemReference(7, 9, 12).NodeReference(7, 8, 'x'));
        }

        /// <summary>
        /// Saving a populated item index page should call through to the IO manager, saving data in the expected format.
        /// </summary>
        [TestMethod]
        public void SavingAPopulatedItemIndexPageShouldCallThroughToIOManager()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var ioManager = new MockDataFileManagerSetup(false)
                                .IndexNodePage(0, null, 2, new ItemRefSetup(2, 5, 88).AndNodeRef(6, 4, 'a'))
                                .ItemPage(1, null, null, new IndexedItemSetup<int>(1, 3))
                                .IndexNodePage(2, 0, 3, new ItemRefSetup(8, 3, 12))
                                .IndexNodePage(3, 2, null, null)
                                .Prepare();

            var persistence = new GenericPersistence<int>();
            var pageManager = CreatePageManager(settings, ioManager, persistence);

            // Execute
            var page = (ItemIndexDataPage<int>)pageManager.GetPage(1);
            page.AddEntry(new ItemEntry<int>(2, 44, 8));
            pageManager.SavePage(page);

            VerifyPersistedData(
                ioManager.GetPageData(1),
                Data.ItemPage(null, null, 2, 1, 2, Data.PageHeaderSize + 8 + 8).IndexedItem(1, 3).IndexedItem(2, 44));
        }
    }
}
