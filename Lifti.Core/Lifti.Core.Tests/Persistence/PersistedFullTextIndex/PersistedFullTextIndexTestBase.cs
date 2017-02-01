// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using Lifti.Extensibility;
    using Lifti.Persistence;

    using Moq;

    /// <summary>
    /// The base class for persisted full text index tests.
    /// </summary>
    public class PersistedFullTextIndexTestBase : UnitTestBase
    {
        /// <summary>
        /// Creates a new persisted full text index instance.
        /// </summary>
        /// <returns>
        /// The new persisted full text index instance.
        /// </returns>
        protected static PersistedFullTextIndex<string> CreatePersistedFullTextIndex()
        {
            var entryManager = new Mock<IPersistedEntryManager<string>>(MockBehavior.Loose);
            return CreatePersistedFullTextIndex(entryManager);
        }

        /// <summary>
        /// Creates a new persisted full text index instance.
        /// </summary>
        /// <param name="entryManager">The entry manager.</param>
        /// <returns>The new persisted full text index instance.</returns>
        protected static PersistedFullTextIndex<string> CreatePersistedFullTextIndex(Mock<IPersistedEntryManager<string>> entryManager)
        {
            return new PersistedFullTextIndex<string>(entryManager.Object, new IndexExtensibilityService<string>());
        }
    }
}