// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageCacheTests
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the page caching mechanism of the <see cref="PageCache"/>.
    /// </summary>
    [TestFixture]
    public class CachingPage
    {
        /// <summary>
        /// The cache should return the page once it has been cached.
        /// </summary>
        [Test]
        public void ShouldCacheAndReturnPageSuccessfully()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CachePage(page.Object);

            Assert.AreEqual(10, cache.GetCachedPage(page.Object.Header, h => (IDataPage)null).Header.PageNumber);
        }

        /// <summary>
        /// The cache should not raise an exception if the page is already cached.
        /// </summary>
        [Test]
        public void ShouldNotRaiseExceptionIfPageAlreadyCached()
        {
            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header.PageNumber).Returns(10);
            
            var page2 = new Mock<IDataPage>();
            page2.SetupGet(p => p.Header.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CachePage(page.Object);
            cache.CachePage(page2.Object);

            Assert.AreEqual(10, cache.GetCachedPage(page.Object.Header, h => (IDataPage)null).Header.PageNumber);
        }

        /// <summary>
        /// If the provided page is null, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfPageIsNull()
        {
            var cache = new PageCache();
            this.AssertRaisesArgumentNullException(() => cache.CachePage(null), "page");
        }
    }
}
