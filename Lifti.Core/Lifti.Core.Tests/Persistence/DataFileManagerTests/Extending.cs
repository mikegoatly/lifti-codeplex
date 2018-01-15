// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.DataFileManagerTests
{
    #region Using statements

    using System;

    using NUnit.Framework;

    using Should;

    #endregion

    /// <summary>
    /// Tests for extending the underlying file using the IO manager.
    /// </summary>
    [TestFixture]
    public class Extending : DataFileManagerTest
    {
        /// <summary>
        /// The underlying file should be extended as requested.
        /// </summary>
        [Test]
        public void ShouldExtendTheLengthOfTheUnderlyingFile()
        {
            this.Sut.ExtendStream(8);

            this.Stream.Length.ShouldEqual(8L);
        }

        /// <summary>
        /// An exception should be thrown if the file is extended to less than its current size.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfRequestedLengthIsLessThanOrEqualToCurrentLength()
        {
            Assert.Throws<ArgumentException>(() => this.Sut.ExtendStream(4), "Can only extend file beyond its current length.\r\nParameter name: requiredLength");
        }
    }
}
