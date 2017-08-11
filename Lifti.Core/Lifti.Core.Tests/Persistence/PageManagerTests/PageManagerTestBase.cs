// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Lifti.Extensibility;
    using Lifti.Persistence;
    using Lifti.Tests.Persistence.PageManagerTests.Setup;

    using NUnit.Framework;

    using Moq;

    /// <summary>
    /// The base class for page manager unit tests.
    /// </summary>
    public abstract class PageManagerTestBase
    {
        /// <summary>
        /// The sequence of bytes that represent an empty/unused data page.
        /// </summary>
        protected static readonly IEnumerable<byte> EmptyPage = Data.Start.Then((byte)DataPageType.Unused).Then(-1, -1).Then((short)0).ToArray();

        /// <summary>
        /// The sequence of bytes that represent an empty item index data page.
        /// </summary>
        protected static readonly IEnumerable<byte> EmptyItemPage = Data.Start.Then((byte)DataPageType.Items).Then(-1, -1).Then((short)0).ToArray();

        /// <summary>
        /// The sequence of bytes that represent an empty item index data page.
        /// </summary>
        protected static readonly IEnumerable<byte> EmptyItemNodeIndexPage = Data.Start.Then((byte)DataPageType.ItemNodeIndex).Then(-1, -1).Then((short)0).ToArray();

        /// <summary>
        /// The sequence of bytes that represent an empty text index data page.
        /// </summary>
        protected static readonly IEnumerable<byte> EmptyIndexNodePage = Data.Start.Then((byte)DataPageType.IndexNode).Then(-1, -1).Then((short)0).ToArray();

        /// <summary>
        /// Creates a memory reader that the page manager will read from.
        /// </summary>
        /// <param name="data">The data that will be read.</param>
        /// <returns>The binary reader.</returns>
        protected static BinaryReader CreateMemoryReader(IEnumerable<byte> data)
        {
            var memoryStream = new MemoryStream(data.ToArray());
            return new BinaryReader(memoryStream);
        }

        /// <summary>
        /// Asserts that the page references are as expected.
        /// </summary>
        /// <param name="page">The page to verify.</param>
        /// <param name="pageNumber">The expected page number.</param>
        /// <param name="previousPage">The expected previous page.</param>
        /// <param name="nextPage">The expected next page.</param>
        protected static void AssertPageReferences(IDataPage page, int pageNumber, int? previousPage, int? nextPage)
        {
            Assert.AreEqual(nextPage, page.Header.NextPage);
            Assert.AreEqual(previousPage, page.Header.PreviousPage);
            Assert.AreEqual(pageNumber, page.Header.PageNumber);
        }

        /// <summary>
        /// Resets the given writer, returning the same instance with a length of 0.
        /// </summary>
        /// <param name="writer">The writer to reset and return.</param>
        /// <returns>The reset instance.</returns>
        protected static BinaryWriter ResetWriter(BinaryWriter writer)
        {
            writer.BaseStream.Position = 0;
            writer.BaseStream.SetLength(0);
            return writer;
        }

        /// <summary>
        /// Verifies the persisted data.
        /// </summary>
        /// <param name="stream">The stream that was written to.</param>
        /// <param name="expectedData">The expected data.</param>
        protected static void VerifyPersistedData(MemoryStream stream, IEnumerable<byte> expectedData)
        {
            var expectedDataArray = expectedData.ToArray();
            var actualData = stream.ToArray();

            Assert.IsTrue(expectedDataArray.Length <= actualData.Length, "Not enough data to compare");
            for (var i = 0; i < expectedDataArray.Length; i++)
            {
                Assert.AreEqual(expectedDataArray[i], actualData[i], "Data differs at index " + i);
            }
        }

        /// <summary>
        /// Creates a memory writer that an index will write to.
        /// </summary>
        /// <returns>The binary writer.</returns>
        protected static BinaryWriter CreateMemoryWriter()
        {
            var memoryStream = new MemoryStream();
            return new BinaryWriter(memoryStream);
        }

        /// <summary>
        /// Creates and initializes a page manager.
        /// </summary>
        /// <param name="settings">The persistence settings.</param>
        /// <param name="ioManager">The IO manager.</param>
        /// <param name="persistence">The type persistence implementation.</param>
        /// <returns>The constructed and initialized page manager instance.</returns>
        protected static PageManager<int> CreatePageManager(Mock<IPersistenceSettings> settings, MockDataFileManagerSetup ioManager, ITypePersistence<int> persistence)
        {
            var extensibilityService = new Mock<IIndexExtensibilityService<int>>();

            var pageManager = new PageManager<int>(new PageCache(), settings.Object, ioManager.Mock.Object, persistence, extensibilityService.Object);
            pageManager.Initialize();
            return pageManager;
        }
    }
}