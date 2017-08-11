// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.TransactionLogFactoryTests
{
    using Lifti.Persistence;
    using Lifti.Persistence.IO;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// Tests the disposing behaviour of the <see cref="TransactionLogFactory"/>
    /// </summary>
    [TestFixture]
    public class Disposing
    {
        /// <summary>
        /// The underlying log file manager should be disposed when the factory is disposed.
        /// </summary>
        [Test]
        public void ShouldDisposeTheLogFile()
        {
            var logFile = new Mock<ILogFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            factory.Dispose();

            logFile.Verify(l => l.Dispose(), Times.Exactly(1));
        }

        /// <summary>
        /// The underlying log file manager should only be disposed once if the dispose
        /// method is called multiple times.
        /// </summary>
        [Test]
        public void MultipleDisposalsShouldDisposeTheLogFileOnce()
        {
            var logFile = new Mock<ILogFileManager>();
            var factory = new TransactionLogFactory(logFile.Object);

            factory.Dispose();
            factory.Dispose();

            logFile.Verify(l => l.Dispose(), Times.Exactly(1));
        }
    }
}
