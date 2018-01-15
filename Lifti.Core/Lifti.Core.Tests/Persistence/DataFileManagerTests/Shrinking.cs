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
    public class Shrinking : DataFileManagerTest
    {
        /// <summary>
        /// An exception should be thrown if the file is shrunk to less than 1.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfRequestedLengthIsEqualToZero()
        {
            Assert.Throws<ArgumentException>(() => this.Sut.ShrinkStream(0), "Required length must be greater than 0.\r\nParameter name: requiredLength");
        }

        /// <summary>
        /// An exception should be thrown if the file is shrunk to less than 0.
        /// </summary>
        [Test]
        public void ShouldRaiseExceptionIfRequestedLengthIsLessThanZero()
        {
            Assert.Throws<ArgumentException>(() => this.Sut.ShrinkStream(-10), "Required length must be greater than 0.\r\nParameter name: requiredLength");
        }

        /// <summary>
        /// The underlying file should be shrunk as requested.
        /// </summary>
        [Test]
        public void ShouldShrinkTheLengthOfTheUnderlyingFile()
        {
            this.Sut.ShrinkStream(4);
            this.Stream.Length.ShouldEqual(4L);
        }
    }
}
