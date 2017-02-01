// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Lifti.Persistence;

    /// <summary>
    /// Extensions for the enumerables of bytes.
    /// </summary>
    internal static class Data
    {
        /// <summary>
        /// The current data file version.
        /// </summary>
        internal const short CurrentDataVersion = 1;

        /// <summary>
        /// The current log file version.
        /// </summary>
        internal const byte CurrentLogVersion = 1;

        /// <summary>
        /// The size of a page.
        /// </summary>
        internal const int PageSize = 8192;

        /// <summary>
        /// The size of header data allocated each page.
        /// </summary>
        internal const int PageHeaderSize = 21;

        /// <summary>
        /// The size of the file header.
        /// </summary>
        internal const int HeaderSize = 8;

        /// <summary>
        /// The size of the page manager-specific file header data.
        /// </summary>
        internal const int PageManagerHeaderSize = 24;

        /// <summary>
        /// The size of the log file header.
        /// </summary>
        internal const int LogHeaderSize = 12;

        /// <summary>
        /// The size of the header data for an entry in a log file.
        /// </summary>
        internal const int LogEntryHeaderSize = 9;

        /// <summary>
        /// The common marker bytes that are stored in the header of the log file.
        /// </summary>
        internal static readonly byte[] LogHeaderBytes = { 0x4C, 0x49, 0x4C, 0x47, 0x4D, 0x47 };

        /// <summary>
        /// The bytes expected at the header of a data file.
        /// </summary>
        internal static readonly byte[] FileHeaderBytes = { 0x4C, 0x49, 0x46, 0x54, 0x4D, 0x47 };

        /// <summary>
        /// An empty byte array - this is a convenience to start an enumerable chain of bytes.
        /// </summary>
        private static readonly byte[] emptyBytes = new byte[0];

        /// <summary>
        /// Gets the empty set of bytes that represents the start of a data sequence.
        /// </summary>
        /// <value>An empty byte array.</value>
        public static IEnumerable<byte> Start
        {
            get
            {
                return emptyBytes;
            }
        }

        /// <summary>
        /// Enumerates the first set of bytes, then the next.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="next">The next bytes.</param>
        /// <returns>The previous bytes followed by the next bytes.</returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, IEnumerable<byte> next)
        {
            return previous.Concat(next);
        }

        /// <summary>
        /// Adds the byte representation of the next value to the preceding bytes.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="next">The next value.</param>
        /// <returns>The previous bytes followed by the next bytes.</returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, byte next)
        {
            foreach (var data in previous)
            {
                yield return data;
            }

            yield return next;
        }

        /// <summary>
        /// Adds the byte representation of the next value to the preceding bytes.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="next">The next value.</param>
        /// <returns>The previous bytes followed by the next bytes.</returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, char next)
        {
            return previous.Concat(Encoding.UTF8.GetBytes(new[] { next }));
        }

        /// <summary>
        /// Adds the byte representation of the next value to the preceding bytes.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="next">The next value.</param>
        /// <returns>The previous bytes followed by the next bytes.</returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, short next)
        {
            return previous.Concat(new[] { (byte)next, (byte)(next >> 8) });
        }

        /// <summary>
        /// Adds the byte representation of the next value to the preceding bytes.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="next">The next value.</param>
        /// <returns>The previous bytes followed by the next bytes.</returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, int next)
        {
            return previous.Concat(new[] { (byte)next, (byte)(next >> 8), (byte)(next >> 16), (byte)(next >> 24) });
        }

        /// <summary>
        /// Adds the byte representation of the next value to the preceding bytes.
        /// </summary>
        /// <param name="previous">The previous bytes.</param>
        /// <param name="nextValues">The next values.</param>
        /// <returns>
        /// The previous bytes followed by the next bytes.
        /// </returns>
        public static IEnumerable<byte> Then(this IEnumerable<byte> previous, params int[] nextValues)
        {
            return nextValues.Aggregate(previous, (current, next) => current.Then(next));
        }

        /// <summary>
        /// Creates the byte sequence for an item index data page.
        /// </summary>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="entryCount">The entry count.</param>
        /// <param name="firstEntry">The first entry in the page.</param>
        /// <param name="lastEntry">The last entry in the page.</param>
        /// <param name="size">The size of the data in the page.</param>
        /// <returns>The constructed byte sequence.</returns>
        public static IEnumerable<byte> ItemPage(int? previousPage, int? nextPage, short entryCount, int firstEntry, int lastEntry, short size)
        {
            return PageHeader(DataPageType.Items, previousPage, nextPage, entryCount, firstEntry, lastEntry, size);
        }

        /// <summary>
        /// Creates the byte sequence for an index node data page.
        /// </summary>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="entryCount">The entry count.</param>
        /// <param name="firstEntry">The first entry in the page.</param>
        /// <param name="lastEntry">The last entry in the page.</param>
        /// <param name="size">The size of the data in the page.</param>
        /// <returns>The constructed byte sequence.</returns>
        public static IEnumerable<byte> IndexNodePage(int? previousPage, int? nextPage, short entryCount, int firstEntry, int lastEntry, short size)
        {
            return PageHeader(DataPageType.IndexNode, previousPage, nextPage, entryCount, firstEntry, lastEntry, size);
        }

        /// <summary>
        /// Creates the byte sequence for an item node index data page.
        /// </summary>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="entryCount">The entry count.</param>
        /// <param name="firstEntry">The first entry in the page.</param>
        /// <param name="lastEntry">The last entry in the page.</param>
        /// <param name="size">The size of the data in the page.</param>
        /// <returns>The constructed byte sequence.</returns>
        public static IEnumerable<byte> ItemNodeIndexPage(int? previousPage, int? nextPage, short entryCount, int firstEntry, int lastEntry, short size)
        {
            return PageHeader(DataPageType.ItemNodeIndex, previousPage, nextPage, entryCount, firstEntry, lastEntry, size);
        }

        /// <summary>
        /// Creates the byte sequence for a data page header.
        /// </summary>
        /// <param name="pageType">Type of the page.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="entryCount">The entry count.</param>
        /// <param name="firstEntry">The first entry in the page.</param>
        /// <param name="lastEntry">The last entry in the page.</param>
        /// <param name="size">The size of the data in the page.</param>
        /// <returns>The constructed byte sequence.</returns>
        public static IEnumerable<byte> PageHeader(DataPageType pageType, int? previousPage, int? nextPage, short entryCount, int firstEntry, int lastEntry, short size)
        {
            return emptyBytes
                .Then((byte)pageType)
                .Then(previousPage ?? -1)
                .Then(nextPage ?? -1)
                .Then(entryCount)
                .Then(firstEntry)
                .Then(lastEntry)
                .Then(size);
        }

        /// <summary>
        /// Creates the byte sequence for an index node data page's node reference.
        /// </summary>
        /// <param name="previous">The previous byte sequence to join on to.</param>
        /// <param name="internalId">The internal id.</param>
        /// <param name="referencedNodeId">The referenced node id.</param>
        /// <param name="referencedCharacter">The referenced character.</param>
        /// <returns>
        /// The constructed byte sequence.
        /// </returns>
        public static IEnumerable<byte> NodeReference(this IEnumerable<byte> previous, int internalId, int referencedNodeId, char referencedCharacter)
        {
            return previous.Then((byte)IndexNodeEntryType.NodeReference).Then(internalId).Then(referencedNodeId).Then(referencedCharacter);
        }

        /// <summary>
        /// Creates the byte sequence for an index node data page's item reference.
        /// </summary>
        /// <param name="previous">The previous byte sequence to join on to.</param>
        /// <param name="internalId">The internal id.</param>
        /// <param name="itemData">The item data.</param>
        /// <returns>
        /// The constructed byte sequence.
        /// </returns>
        public static IEnumerable<byte> IndexedItem(this IEnumerable<byte> previous, int internalId, int itemData)
        {
            return previous.Then(internalId).Then(itemData);
        }

        /// <summary>
        /// Creates the byte sequence for an item index data page's item reference.
        /// </summary>
        /// <param name="previous">The previous byte sequence to join on to.</param>
        /// <param name="internalId">The internal id.</param>
        /// <param name="referencedItemId">The referenced item id.</param>
        /// <param name="wordPosition">The word position.</param>
        /// <returns>
        /// The constructed byte sequence.
        /// </returns>
        public static IEnumerable<byte> ItemReference(this IEnumerable<byte> previous, int internalId, int referencedItemId, int wordPosition)
        {
            return previous.Then((byte)IndexNodeEntryType.ItemReference).Then(internalId).Then(referencedItemId).Then(wordPosition);
        }

        /// <summary>
        /// Creates a binary representation of a data file header.
        /// </summary>
        /// <param name="logState">State of the log.</param>
        /// <param name="originalExtent">The original extent to record in the header.</param>
        /// <returns>The relevant sequence of byte data.</returns>
        public static IEnumerable<byte> DataFileHeader(TransactionLogState logState, int originalExtent)
        {
            return FileHeaderBytes.Then(CurrentDataVersion);
        }

        /// <summary>
        /// Creates a binary representation of a log file header.
        /// </summary>
        /// <param name="logState">State of the log.</param>
        /// <param name="originalExtent">The original extent to record in the header.</param>
        /// <returns>The relevant sequence of byte data.</returns>
        public static IEnumerable<byte> LogFileHeader(TransactionLogState logState, int originalExtent)
        {
            return LogHeaderBytes.Then(CurrentLogVersion).Then((byte)logState).Then(originalExtent);
        }

        /// <summary>
        /// Gets a byte representation of a log file entry header.
        /// </summary>
        /// <param name="entryType">Type of the entry.</param>
        /// <param name="offset">The logged offset.</param>
        /// <param name="length">The logged length.</param>
        /// <returns>The byte representation of the header.</returns>
        public static IEnumerable<byte> LogEntryHeader(LogEntryDataType entryType, int offset, int length)
        {
            return Start.Then((byte)entryType).Then(offset).Then(length);
        }
    }
}
