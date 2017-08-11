// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using Lifti.Locking;
    using Lifti.Querying;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests that exercise the LockManager integration with the full text index.
    /// </summary>
    [TestFixture]
    public class FullTextIndexLockTests
    {
        /// <summary>
        /// Tests that the Index method obtains a write lock.
        /// </summary>
        [Test]
        public void IndexItemMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Index(this.GetType(), t => t.Name, t => t.Name);

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Index method obtains a write lock.
        /// </summary>
        [Test]
        public void IndexItesmMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Index(new[] { this.GetType() }, t => t.Name, t => t.Name);

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Index method obtains a write lock.
        /// </summary>
        [Test]
        public void IndexMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Index("Test", "Test");

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Index method obtains a write lock.
        /// </summary>
        [Test]
        public void IndexManyMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Index(new[] { "Test", "Test2" }, s => s);

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Remove method obtains a write lock.
        /// </summary>
        [Test]
        public void RemoveMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Remove("Test");

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Remove method obtains a write lock.
        /// </summary>
        [Test]
        public void RemoveManyMethodShouldObtainWriteLock()
        {
            var writeLock = new Mock<ILock>(MockBehavior.Strict);
            writeLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireWriteLock()).Returns(writeLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Remove(new[] { "Test", "Test2" });

            lockManager.VerifyAll();
            writeLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Search method obtains a read lock.
        /// </summary>
        [Test]
        public void SearchMethodShouldObtainReadLock()
        {
            var readLock = new Mock<ILock>(MockBehavior.Strict);
            readLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireReadLock()).Returns(readLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Search("Test");

            lockManager.VerifyAll();
            readLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Search method that takes a pre-compiled query obtains a read lock.
        /// </summary>
        [Test]
        public void QuerySearchMethodShouldObtainReadLock()
        {
            var readLock = new Mock<ILock>(MockBehavior.Strict);
            readLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireReadLock()).Returns(readLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Search(new FullTextQuery(new ExactWordQueryPart("Test")));

            lockManager.VerifyAll();
            readLock.VerifyAll();
        }

        /// <summary>
        /// Tests that the Contains method obtains a read lock.
        /// </summary>
        [Test]
        public void ContainsShouldObtainReadLock()
        {
            var readLock = new Mock<ILock>(MockBehavior.Strict);
            readLock.Setup(l => l.Dispose());
            var lockManager = new Mock<ILockManager>(MockBehavior.Strict);
            lockManager.Setup(l => l.AcquireReadLock()).Returns(readLock.Object);

            var index = new UpdatableFullTextIndex<string> { LockManager = lockManager.Object };

            index.Contains("Test");

            lockManager.VerifyAll();
            readLock.VerifyAll();
        }
    }
}
