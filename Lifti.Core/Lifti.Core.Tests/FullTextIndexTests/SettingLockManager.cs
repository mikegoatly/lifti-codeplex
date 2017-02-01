// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.FullTextIndexTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests the setting of the LockManager property.
    /// </summary>
    [TestClass]
    public class SettingLockManager : FullTextIndexTestBase
    {
        /// <summary>
        /// Tests that attempting to set the LockManager of a FullTextIndex should raise
        /// an ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void SettingLockManagerToNullShouldRaiseException()
        {
            var index = new FullTextIndex<Customer>();
            AssertRaisesArgumentNullException(() => index.LockManager = null, "value");
        }
    }
}
