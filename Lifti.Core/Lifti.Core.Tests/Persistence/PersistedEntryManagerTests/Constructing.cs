// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedEntryManagerTests
{
    using Lifti.Persistence;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for the constructing of the persisted entry manager.
    /// </summary>
    [TestFixture]
    public class Constructing
    {
        /// <summary>
        /// The entry manager should initialize the page manager if it is not already initialized.
        /// </summary>
        [Test]
        public void ShouldInitializePageManagerIfItIsNotAlreadyInitialized()
        {
            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(m => m.Initialized).Returns(false);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Verify(m => m.Initialize(), Times.Exactly(1));
        }

        /// <summary>
        /// The entry manager should not initialize the page manager if it is already initialized.
        /// </summary>
        [Test]
        public void ShouldNotInitializePageManagerIfItIsAlreadyInitialized()
        {
            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(m => m.Initialized).Returns(true);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            pageManager.Verify(m => m.Initialize(), Times.Never());
        }

        /// <summary>
        /// The entry manager should store the reference to the given page manager.
        /// </summary>
        [Test]
        public void ShouldStoreReferenceToPageManager()
        {
            var pageManager = new Mock<IPageManager>();
            pageManager.SetupGet(m => m.Initialized).Returns(true);

            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);
            var manager = new PersistedEntryManager<int>(pageManager.Object, typePersistence.Object);

            Assert.AreSame(pageManager.Object, manager.PageManager);
        }
    }
}
