// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using System.Linq;

    using Lifti.Extensibility;
    using Lifti.Persistence;
    using Lifti.Persistence.IO;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the construction process of the 
    /// </summary>
    [TestClass]
    public class Constructing : PageManagerTestBase
    {
        /// <summary>
        /// If a null page cache is provided to the constructor, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionForNullPageCache()
        {
            AssertRaisesArgumentNullException(
                () => new PageManager<int>(null, new Mock<IPersistenceSettings>().Object, new Mock<IDataFileManager>().Object, new Mock<ITypePersistence<int>>().Object, new Mock<IIndexExtensibilityService<int>>().Object),
                "pageCache");
        }

        /// <summary>
        /// If a null persistence settings is provided to the constructor, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionForNullPersistenceSettings()
        {
            AssertRaisesArgumentNullException(
                () => new PageManager<int>(new Mock<IPageCache>().Object, null, new Mock<IDataFileManager>().Object, new Mock<ITypePersistence<int>>().Object, new Mock<IIndexExtensibilityService<int>>().Object),
                "settings");
        }

        /// <summary>
        /// If a null IO manager is provided to the constructor, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionForNullIOManager()
        {
            AssertRaisesArgumentNullException(
                () => new PageManager<int>(new Mock<IPageCache>().Object, new Mock<IPersistenceSettings>().Object, null, new Mock<ITypePersistence<int>>().Object, new Mock<IIndexExtensibilityService<int>>().Object),
                "dataFileManager");
        }

        /// <summary>
        /// If a null type persistence is provided to the constructor, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionForNullTypePersistenceSettings()
        {
            AssertRaisesArgumentNullException(
                () => new PageManager<int>(new Mock<IPageCache>().Object, new Mock<IPersistenceSettings>().Object, new Mock<IDataFileManager>().Object, null, new Mock<IIndexExtensibilityService<int>>().Object),
                "typePersistence");
        }

        /// <summary>
        /// If a null extensibility service is provided to the constructor, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionForNullExtensibilityService()
        {
            AssertRaisesArgumentNullException(
                () => new PageManager<int>(new Mock<IPageCache>().Object, new Mock<IPersistenceSettings>().Object, new Mock<IDataFileManager>().Object, new Mock<ITypePersistence<int>>().Object, null),
                "extensibilityService");
        }

        /// <summary>
        /// A persisted index should initialize the backing store automatically if is loading for the first time.
        /// </summary>
        [TestMethod]
        public void ShouldAutomaticallyInitializeNewIndexIfLoadingForTheFirstTime()
        {
            // Setup
            const int InitializePageCount = 4;
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            settings.SetupGet(s => s.GrowPageCount).Returns(InitializePageCount);

            var ioManager = new MockDataFileManagerSetup(true).Prepare();

            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();
            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            // Execute
            var pageManager = new PageManager<int>(new PageCache(), settings.Object, ioManager.Mock.Object, persistence.Object, extensibilityService.Object);
            pageManager.Initialize();

            // Verify
            Assert.AreEqual(1, pageManager.UnusedPageCount);
            Assert.AreEqual(InitializePageCount, pageManager.TotalPageCount);
            Assert.AreEqual(0, pageManager.ItemDataPages.Single().PageNumber);
            Assert.AreEqual(1, pageManager.IndexNodeDataPages.Single().PageNumber);
            Assert.AreEqual(2, pageManager.ItemNodeIndexDataPages.Single().PageNumber);

            VerifyPersistedData(ioManager.FileHeader, Data.FileHeaderBytes);
            VerifyPersistedData(ioManager.PageManagerHeader, Data.Start.Then(0, 1, 2, InitializePageCount, 0, 1));
            VerifyPersistedData(ioManager.GetPageData(0), EmptyItemPage);
            VerifyPersistedData(ioManager.GetPageData(1), EmptyIndexNodePage);
            VerifyPersistedData(ioManager.GetPageData(2), EmptyItemNodeIndexPage);
            VerifyPersistedData(ioManager.GetPageData(3), EmptyPage);

            settings.VerifyAll();
            ioManager.VerifyBasics();
            persistence.VerifyAll();
        }

        /// <summary>
        /// A persisted index should construct successfully when a valid index is loaded that has empty pages at the end.
        /// </summary>
        [TestMethod]
        public void ShouldConstructSuccessfullyWhenIndexWithEmptyPagesIsLoaded()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);

            var ioManager = new MockDataFileManagerSetup(false)
                .IndexNodePage(0, null, null, null)
                .ItemPage<int>(1, null, null, null)
                .EmptyPage(2)
                .EmptyPage(3)
                .Prepare();

            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();
            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            // Execute
            var pageManager = new PageManager<int>(new PageCache(), settings.Object, ioManager.Mock.Object, persistence.Object, extensibilityService.Object);
            pageManager.Initialize();

            // Verify
            Assert.AreEqual(2, pageManager.UnusedPageCount);
            Assert.AreEqual(5, pageManager.TotalPageCount);
            Assert.AreEqual(1, pageManager.ItemDataPages.Single().PageNumber);
            Assert.AreEqual(0, pageManager.IndexNodeDataPages.Single().PageNumber);
            Assert.AreEqual(4, pageManager.ItemNodeIndexDataPages.Single().PageNumber);

            settings.VerifyAll();
            ioManager.VerifyBasics();
            ioManager.VerifyNeverExtended();
            persistence.VerifyAll();
        }

        /// <summary>
        /// A persisted index should construct successfully when a valid and an index with no empty pages is loaded.
        /// </summary>
        [TestMethod]
        public void ShouldConstructSuccessfullyWhenFullyPopulatedIndexIsLoaded()
        {
            // Setup
            var settings = new Mock<IPersistenceSettings>(MockBehavior.Strict);

            var ioManager = new MockDataFileManagerSetup(false)
                .IndexNodePage(0, null, 2, new NodeRefSetup(1, 2, 'a').AndNodeRef(1, 3, 'b').AndItemRef(2, 1, 9).AndItemRef(3, 2, 1))
                .ItemPage(1, null, 3, new IndexedItemSetup<int>(1, 55).AndItem(2, 99))
                .IndexNodePage(2, 0, null, new ItemRefSetup(3, 2, 0).AndItemRef(3, 3, 9))
                .ItemPage(3, 1, null, new IndexedItemSetup<int>(3, 67))
                .Prepare();

            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();
            var persistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            // Execute
            var pageManager = new PageManager<int>(new PageCache(), settings.Object, ioManager.Mock.Object, persistence.Object, extensibilityService.Object);
            pageManager.Initialize();

            // Verify
            Assert.AreEqual(0, pageManager.UnusedPageCount);
            Assert.AreEqual(5, pageManager.TotalPageCount);
            Assert.IsTrue((new[] { 1, 3 }).SequenceEqual(pageManager.ItemDataPages));
            Assert.IsTrue((new[] { 0, 2 }).SequenceEqual(pageManager.IndexNodeDataPages));
            Assert.IsTrue((new[] { 4 }).SequenceEqual(pageManager.ItemNodeIndexDataPages));

            settings.VerifyAll();
            ioManager.VerifyBasics();
            ioManager.VerifyNeverExtended();
            persistence.VerifyAll();
        }
    }
}
