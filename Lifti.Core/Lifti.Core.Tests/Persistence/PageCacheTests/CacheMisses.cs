// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageCacheTests
{
    using Lifti.Persistence;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// Tests for the cases where cache misses occur.
    /// </summary>
    [TestClass]
    public class CacheMisses : UnitTestBase
    {
        /// <summary>
        /// The page body should be loaded using the provided delegate if it is
        /// not found in the cache.
        /// </summary>
        [TestMethod]
        public void ShouldLoadPageIfNotFoundInCache()
        {
            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CacheHeader(pageHeader.Object);

            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header).Returns(pageHeader.Object);

            Assert.AreEqual(10, cache.GetCachedPage(pageHeader.Object, h => page.Object).Header.PageNumber);
        }

        /// <summary>
        /// The page body should not be loaded using the delegate if requested a second time
        /// as it should be cached.
        /// </summary>
        [TestMethod]
        public void ShouldNotLoadPageIfAlreadyCached()
        {
            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CacheHeader(pageHeader.Object);

            var page = new Mock<IDataPage>();
            page.SetupGet(p => p.Header).Returns(pageHeader.Object);

            Assert.AreEqual(10, cache.GetCachedPage(pageHeader.Object, h => page.Object).Header.PageNumber);
            Assert.AreEqual(10, cache.GetCachedPage(pageHeader.Object, h => (IDataPage)null).Header.PageNumber);
        }

        /// <summary>
        /// The page header should be loaded using the provided delegate if it is
        /// not found in the cache.
        /// </summary>
        [TestMethod]
        public void ShouldLoadPageHeaderIfNotFoundInCache()
        {
            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();

            Assert.AreEqual(10, cache.GetHeader(10, h => pageHeader.Object).PageNumber);
        }

        /// <summary>
        /// The page header should not be loaded using the delegate if requested a second time
        /// as it should be cached.
        /// </summary>
        [TestMethod]
        public void ShouldNotLoadPageHeaderIfAlreadyCached()
        {
            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();

            Assert.AreEqual(10, cache.GetHeader(10, h => pageHeader.Object).PageNumber);
            Assert.AreEqual(10, cache.GetHeader(10, h => (IDataPageHeader)null).PageNumber);
        }

        /// <summary>
        /// The cache should not return a header if other headers have been cached, but it hasn't.
        /// </summary>
        [TestMethod]
        public void ShouldNotReturnHeaderIfOtherHeaderCached()
        {
            var pageHeader = new Mock<IDataPageHeader>();
            pageHeader.SetupGet(p => p.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CacheHeader(pageHeader.Object);

            AssertRaisesException<PersistenceException>(() => cache.GetHeader(11), "Internal error - an un-cached header was requested. (Page number 11)");
        }

        /// <summary>
        /// If the cache is empty, no headers should be returned.
        /// </summary>
        [TestMethod]
        public void ShouldNotReturnPageHeaderIfCacheEmpty()
        {
            var cache = new PageCache();
            AssertRaisesException<PersistenceException>(() => cache.GetHeader(11), "Internal error - an un-cached header was requested. (Page number 11)");
        }

        /// <summary>
        /// Due to the fact that headers are generally cached forever, the GetHeader method
        /// should raise an exception if a header is requested and it doesn't exist.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfHeaderRequestedAndItIsNotCached()
        {
            var cache = new PageCache();
            AssertRaisesException<PersistenceException>(() => cache.GetHeader(11), "Internal error - an un-cached header was requested. (Page number 11)");
        }

        /// <summary>
        /// If the provided page load delegate is null, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfLoadPageDelegateIsNull()
        {
            var cache = new PageCache();
            AssertRaisesArgumentNullException(() => cache.GetCachedPage(new Mock<IDataPageHeader>().Object, null), "loadPage");
        }

        /// <summary>
        /// If the provided page header is null, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfPageHeaderIsNull()
        {
            var cache = new PageCache();
            AssertRaisesArgumentNullException(() => cache.GetCachedPage(null, h => null), "pageHeader");
        }

        /// <summary>
        /// If the provided load page header delegate is null, an exception should be thrown.
        /// </summary>
        [TestMethod]
        public void ShouldRaiseExceptionIfLoadPageHeaderDelegateIsNull()
        {
            var cache = new PageCache();
            AssertRaisesArgumentNullException(() => cache.GetHeader(1, null), "loadHeader");
        }
    }
}
