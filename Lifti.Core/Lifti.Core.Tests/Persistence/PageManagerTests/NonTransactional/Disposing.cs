// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests for disposing a page manager instance.
    /// </summary>
    [TestFixture]
    public class Disposing : PageManagerTestBase
    {
        /// <summary>
        /// When a page manager is disposed it should dispose the underlying data file manager.
        /// </summary>
        [Test]
        public void ShouldDisposeDataFileManager()
        {
            var dataFileManager =
                new MockDataFileManagerSetup(false).IndexNodePage(0, null, null, null).ItemPage<int>(1, null, null, null).EmptyPage(2).
                    EmptyPage(3).Prepare();

            var persistenceSettings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            dataFileManager.Mock.Setup(m => m.Dispose());

            var pageManager = CreatePageManager(persistenceSettings, dataFileManager, typePersistence.Object);

            pageManager.Dispose();

            dataFileManager.Mock.Verify(m => m.Dispose(), Times.Exactly(1));
        }

        /// <summary>
        /// If the page manager is disposed multiple times, it should still only dispose the data file manager once.
        /// </summary>
        [Test]
        public void ShouldNotDisposeDataFileManagerMultipleTimes()
        {
            var dataFileManager =
                new MockDataFileManagerSetup(false).IndexNodePage(0, null, null, null).ItemPage<int>(1, null, null, null).EmptyPage(2).
                    EmptyPage(3).Prepare();

            var persistenceSettings = new Mock<IPersistenceSettings>(MockBehavior.Strict);
            var typePersistence = new Mock<ITypePersistence<int>>(MockBehavior.Strict);

            dataFileManager.Mock.Setup(m => m.Dispose());

            var pageManager = CreatePageManager(persistenceSettings, dataFileManager, typePersistence.Object);

            pageManager.Dispose();
            pageManager.Dispose();

            dataFileManager.Mock.Verify(m => m.Dispose(), Times.Exactly(1));
        }
    }
}
