// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using Lifti.Persistence;

    using Moq;

    /// <summary>
    /// The base test class for persisted entry manager tests.
    /// </summary>
    public class PersistedEntryManagerTestBase
    {
        /// <summary>
        /// Gets or sets the page 0.
        /// </summary>
        /// <value>
        /// The first page.
        /// </value>
        protected IndexNodeDataPage IndexNodePage0
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the page 1.
        /// </summary>
        /// <value>
        /// The second page.
        /// </value>
        protected IndexNodeDataPage IndexNodePage1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the page 2.
        /// </summary>
        /// <value>
        /// The third page.
        /// </value>
        protected IndexNodeDataPage IndexNodePage2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the entries on page 2.
        /// </summary>
        /// <value>
        /// The entries on page 2.
        /// </value>
        protected IndexNodeEntryBase[] IndexNodePage2Entries { get; set; }

        /// <summary>
        /// Gets or sets the entries on page 1.
        /// </summary>
        /// <value>
        /// The entries on page 1.
        /// </value>
        protected IndexNodeEntryBase[] IndexNodePage1Entries { get; set; }

        /// <summary>
        /// Gets or sets the entries on page 0.
        /// </summary>
        /// <value>
        /// The entries on page 0.
        /// </value>
        protected IndexNodeEntryBase[] IndexNodePage0Entries { get; set; }

        /// <summary>
        /// Creates a mocked page manager.
        /// </summary>
        /// <returns>The mocked page manager.</returns>
        protected static Mock<IPageManager> CreateMockedPageManager()
        {
            var pageManager = new Mock<IPageManager>(MockBehavior.Strict);
            pageManager.SetupGet(m => m.Initialized).Returns(false);
            pageManager.Setup(m => m.Initialize());
            return pageManager;
        }

        /// <summary>
        /// Sets up a page manager containing multiple (simple) pages.
        /// </summary>
        /// <returns>The mocked page manager.</returns>
        protected Mock<IPageManager> SetupMultipageNodeCollectionPageManager()
        {
            var pageManager = CreateMockedPageManager();
            var indexNodeHeaders = new[]
                {
                    new DataPageHeader(DataPageType.IndexNode, 0, null, 1, 2, 0, 1, Data.PageHeaderSize + 10 + 10),
                    new DataPageHeader(DataPageType.IndexNode, 1, 0, 2, 3, 1, 1, Data.PageHeaderSize + 10 + 13 + 10),
                    new DataPageHeader(DataPageType.IndexNode, 2, 1, null, 2, 1, 4, Data.PageHeaderSize + 10 + 10)
                };

            this.IndexNodePage0Entries = new IndexNodeEntryBase[] 
                {
                    new ItemReferenceIndexNodeEntry(0, 3, 5),
                    new ItemReferenceIndexNodeEntry(1, 3, 5)
                };

            this.IndexNodePage1Entries = new IndexNodeEntryBase[]
                {
                    new ItemReferenceIndexNodeEntry(1, 4, 5),
                    new ItemReferenceIndexNodeEntry(1, 4, 9),
                    new NodeReferenceIndexNodeEntry(1, 3, 'b')
                };

            this.IndexNodePage2Entries = new IndexNodeEntryBase[]
                {
                    new ItemReferenceIndexNodeEntry(1, 8, 5),
                    new ItemReferenceIndexNodeEntry(4, 8, 9)
                };

            this.IndexNodePage0 = new IndexNodeDataPage(indexNodeHeaders[0], this.IndexNodePage0Entries);
            this.IndexNodePage1 = new IndexNodeDataPage(indexNodeHeaders[1], this.IndexNodePage1Entries);
            this.IndexNodePage2 = new IndexNodeDataPage(indexNodeHeaders[2], this.IndexNodePage2Entries);

            pageManager.Setup(p => p.GetPage(indexNodeHeaders[0])).Returns(this.IndexNodePage0);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[1])).Returns(this.IndexNodePage1);
            pageManager.Setup(p => p.GetPage(indexNodeHeaders[2])).Returns(this.IndexNodePage2);

            pageManager.SetupGet(p => p.IndexNodeDataPages).Returns(new DataPageCollection(indexNodeHeaders));

            return pageManager;
        }
    }
}
