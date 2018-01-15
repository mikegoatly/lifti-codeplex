// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageCacheTests
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for purging headers from a <see cref="PageCache"/> instance.
    /// </summary>
    [TestFixture]
    public class PurgingHeaders
    {
        /// <summary>
        /// If a request for a single header to be purged is made, it should be honoured.
        /// </summary>
        [Test]
        public void ShouldRemoveSingleHeaderFromCache()
        {
            var header = new Mock<IDataPageHeader>();
            header.SetupGet(p => p.PageNumber).Returns(10);

            var header2 = new Mock<IDataPageHeader>();
            header2.SetupGet(p => p.PageNumber).Returns(11);

            var cache = new PageCache();
            cache.CacheHeader(header.Object);
            cache.CacheHeader(header2.Object);

            // Remove the first header
            cache.PurgeHeaders(new[] { 10 });

            // Verify the header is gone, and the other header still exists
            Assert.IsNull(cache.GetHeader(10, h => null));
            Assert.AreSame(header2.Object, cache.GetHeader(11));
        }

        /// <summary>
        /// If a request for multiple headers to be purged is made, it should be honoured.
        /// </summary>
        [Test]
        public void ShouldRemoveMultipleHeadersFromCache()
        {
            var header = new Mock<IDataPageHeader>();
            header.SetupGet(p => p.PageNumber).Returns(10);

            var header2 = new Mock<IDataPageHeader>();
            header2.SetupGet(p => p.PageNumber).Returns(11);

            var cache = new PageCache();
            cache.CacheHeader(header.Object);
            cache.CacheHeader(header2.Object);

            // Remove the both headers
            cache.PurgeHeaders(new[] { 10, 11 });

            // Verify both headers are gone
            Assert.IsNull(cache.GetHeader(10, h => null));
            Assert.IsNull(cache.GetHeader(11, h => null));
        }

        /// <summary>
        /// If a request to purge a header that isn't currently cached, it should have no effect.
        /// </summary>
        [Test]
        public void ShouldHaveNoEffectIfPurgedHeadersNotInCache()
        {
            var header = new Mock<IDataPageHeader>();
            header.SetupGet(p => p.PageNumber).Returns(10);

            var header2 = new Mock<IDataPageHeader>();
            header2.SetupGet(p => p.PageNumber).Returns(11);

            var header3 = new Mock<IDataPageHeader>();
            header3.SetupGet(p => p.PageNumber).Returns(12);

            // Cache only the first two headers
            var cache = new PageCache();
            cache.CacheHeader(header.Object);
            cache.CacheHeader(header2.Object);

            // Remove the last 2 headers
            cache.PurgeHeaders(new[] { 11, 12 });

            // Verify only the first header remains
            Assert.AreSame(header.Object, cache.GetHeader(10));
            Assert.IsNull(cache.GetHeader(11, h => null));
            Assert.IsNull(cache.GetHeader(12, h => null));
        }

        /// <summary>
        /// When purging headers, associated pages should not be purged.
        /// </summary>
        [Test]
        public void ShouldOnlyPurgeHeadersNotAssociatedPages()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);

            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CacheHeader(pageHeader.Object);

            // Remove the first header
            cache.PurgeHeaders(new[] { 10 });

            // Verify the header is gone, but the page still exists
            Assert.AreSame(page.Object, cache.GetCachedPage(page.Object.Header, h => null));
            Assert.IsNull(cache.GetHeader(10, h => null));
        }

        /// <summary>
        /// If the provided list of headers to purge is null, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfListOfHeadersIsNull()
        {
            var cache = new PageCache();
            this.AssertRaisesArgumentNullException(() => cache.PurgeHeaders(null), "headers");
        }
    }
}
