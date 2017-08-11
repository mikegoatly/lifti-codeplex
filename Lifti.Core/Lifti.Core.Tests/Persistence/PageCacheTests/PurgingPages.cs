// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageCacheTests
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for purging pages from a <see cref="PageCache"/> instance.
    /// </summary>
    [TestFixture]
    public class PurgingPages
    {
        /// <summary>
        /// If a request for a single page to be purged is made, it should be honoured.
        /// </summary>
        [Test]
        public void ShouldRemoveSinglePageFromCache()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);
            
            var page2 = new Mock<IDataPage>();
            page2.SetupGet(p => p.Header.PageNumber).Returns(11);

            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CachePage(page2.Object);

            // Remove the first page
            cache.PurgePages(new[] { 10 });

            // Verify the page is gone, and the other page still exists
            Assert.IsFalse(cache.IsPageCached(10));
            Assert.IsTrue(cache.IsPageCached(11));
        }

        /// <summary>
        /// If a request for multiple pages to be purged is made, it should be honoured.
        /// </summary>
        [Test]
        public void ShouldRemoveMultiplePagesFromCache()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);

            var page2 = new Mock<IDataPage>();
            page2.SetupGet(p => p.Header.PageNumber).Returns(11);

            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CachePage(page2.Object);

            // Remove the both pages
            cache.PurgePages(new[] { 10, 11 });

            // Verify both pages are gone
            Assert.IsFalse(cache.IsPageCached(10));
            Assert.IsFalse(cache.IsPageCached(11));
        }

        /// <summary>
        /// If a request to purge a page that isn't currently cached, it should have no effect.
        /// </summary>
        [Test]
        public void ShouldHaveNoEffectIfPurgedPagesNotInCache()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);

            var page2 = new Mock<IDataPage>();
            page2.SetupGet(p => p.Header.PageNumber).Returns(11);

            var page3 = new Mock<IDataPage>();
            page3.SetupGet(p => p.Header.PageNumber).Returns(12);

            // Cache only the first two pages
            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CachePage(page2.Object);

            // Remove the last 2 pages
            cache.PurgePages(new[] { 11, 12 });

            // Verify only the first page remains
            Assert.IsTrue(cache.IsPageCached(10));
            Assert.IsFalse(cache.IsPageCached(11));
            Assert.IsFalse(cache.IsPageCached(12));
        }

        /// <summary>
        /// When purging pages, associated headers should not be purged.
        /// </summary>
        [Test]
        public void ShouldOnlyPurgePagesNotAssociatedHeaders()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);

            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CacheHeader(pageHeader.Object);

            // Remove the first page
            cache.PurgePages(new[] { 10 });

            // Verify the page is gone, but the header still exists
            Assert.IsFalse(cache.IsPageCached(10));
            Assert.AreSame(pageHeader.Object, cache.GetHeader(10));
        }

        /// <summary>
        /// If the provided list of pages to purge is null, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfListOfPagesIsNull()
        {
            var cache = new PageCache();
            this.AssertRaisesArgumentNullException(() => cache.PurgePages(null), "pages");
        }
    }
}
