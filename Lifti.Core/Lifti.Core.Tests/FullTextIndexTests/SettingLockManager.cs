// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using NUnit.Framework;

    /// <summary>
    /// Tests the setting of the LockManager property.
    /// </summary>
    [TestFixture]
    public class SettingLockManager : FullTextIndexTestBase
    {
        /// <summary>
        /// Tests that attempting to set the LockManager of a FullTextIndex should raise
        /// an ArgumentNullException.
        /// </summary>
        [Test]
        public void SettingLockManagerToNullShouldRaiseException()
        {
            var index = new FullTextIndex<Customer>();
            this.AssertRaisesArgumentNullException(() => index.LockManager = null, "value");
        }
    }
}
