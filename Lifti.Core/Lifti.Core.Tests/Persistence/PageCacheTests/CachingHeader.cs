// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageCacheTests
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the header caching mechanism of the <see cref="PageCache"/>.
    /// </summary>
    [TestFixture]
    public class CachingHeader
    {
        /// <summary>
        /// The cache should return the header once it has been cached.
        /// </summary>
        [Test]
        public void ShouldCacheAndReturnHeaderSuccessfully()
        {
            var header = new Mock<IDataPageHeader>();
            header.SetupGet(h => h.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CacheHeader(header.Object);

            Assert.AreSame(header.Object, cache.GetHeader(10));
        }

        /// <summary>
        /// The cache should not raise an exception if the header is already cached.
        /// </summary>
        [Test]
        public void ShouldNotRaiseExceptionIfHeaderAlreadyCached()
        {
            var header = new Mock<IDataPageHeader>();
            header.SetupGet(h => h.PageNumber).Returns(10);

            var header2 = new Mock<IDataPageHeader>();
            header2.SetupGet(h => h.PageNumber).Returns(10);

            var cache = new PageCache();
            cache.CacheHeader(header.Object);
            cache.CacheHeader(header2.Object);

            Assert.AreSame(header2.Object, cache.GetHeader(10));
        }

        /// <summary>
        /// If the provided header is null, an exception should be thrown.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfHeaderIsNull()
        {
            var cache = new PageCache();
            this.AssertRaisesArgumentNullException(() => cache.CacheHeader(null), "header");
        }
    }
}
