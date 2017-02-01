// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence.IO
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Extension methods for the reading and writing of binary data to and from binary readers/writers.
    /// </summary>
    internal static class IOExtensions
    {
        /// <summary>
        /// Restores this instance from the specified reader.
        /// </summary>
        /// <param name="dataReader">The <see cref="BinaryReader"/> to read from.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>The restored data page header.</returns>
        public static IDataPageHeader RestorePageHeader(this BinaryReader dataReader, int pageNumber)
        {
            var dataPageType = (DataPageType)dataReader.ReadByte();
            int? previousPage = null;
            int? nextPage = null;
            var firstId = 0;
            var lastId = 0;
            var entryCount = 0;
            short currentSize = DataFileManager.PageHeaderSize;

            if (dataPageType != DataPageType.Unused)
            {
                var previous = dataReader.ReadInt32();
                var next = dataReader.ReadInt32();
                previousPage = previous == -1 ? (int?)null : previous;
                nextPage = next == -1 ? (int?)null : next;
                entryCount = dataReader.ReadInt16();
                firstId = dataReader.ReadInt32();
                lastId = dataReader.ReadInt32();
                currentSize = dataReader.ReadInt16();
            }

            return new DataPageHeader(dataPageType, pageNumber, previousPage, nextPage, entryCount, firstId, lastId, currentSize);
        }

        /// <summary>
        /// Persists this instance to the specified writer.
        /// </summary>
        /// <param name="dataWriter">The <see cref="BinaryWriter"/> to write to.</param>
        /// <param name="header">The header to persist.</param>
        public static void PersistPageHeader(this BinaryWriter dataWriter, IDataPageHeader header)
        {
            dataWriter.Write((byte)header.DataPageType);
            dataWriter.Write(header.PreviousPage ?? -1);
            dataWriter.Write(header.NextPage ?? -1);
            dataWriter.Write((short)header.EntryCount);
            dataWriter.Write(header.FirstEntry);
            dataWriter.Write(header.LastEntry);
            dataWriter.Write(header.CurrentSize);
        }

        /// <summary>
        /// Restores the data page from the specified data reader.
        /// </summary>
        /// <param name="dataReader">The binary reader to read the page data from. This will
        /// be located at the first byte after the page's header.</param>
        /// <param name="header">The header of the data page.</param>
        /// <returns>The restored page.</returns>
        public static ItemNodeIndexDataPage RestoreItemNodeIndexDataPage(this BinaryReader dataReader, IDataPageHeader header)
        {
            var count = header.EntryCount;
            var entries = new List<ItemNodeIndexEntry>(header.EntryCount);
            for (var i = 0; i < count; i++)
            {
                var entryId = dataReader.ReadInt32();
                entries.Add(new ItemNodeIndexEntry(entryId, dataReader.ReadInt32()));
            }

            return new ItemNodeIndexDataPage(header, entries);
        }

        /// <summary>
        /// Restores the data page from the specified data reader.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="dataReader">The binary reader to read the page data from. This will
        /// be located at the first byte after the page's header.</param>
        /// <param name="header">The header of the data page.</param>
        /// <param name="typePersistence">The type persistence implementation for TItem.</param>
        /// <returns>
        /// The restored page.
        /// </returns>
        public static ItemIndexDataPage<TItem> RestoreItemIndexDataPage<TItem>(this BinaryReader dataReader, IDataPageHeader header, ITypePersistence<TItem> typePersistence)
        {
            System.Diagnostics.Debug.WriteLine("Restoring page " + header.PageNumber + " - " + header.EntryCount);

            var count = header.EntryCount;
            var entries = new List<ItemEntry<TItem>>(header.EntryCount);
            for (var i = 0; i < count; i++)
            {
                var entryId = dataReader.ReadInt32();

                var item = typePersistence.DataReader(dataReader);
                var size = typePersistence.SizeReader(item) + 4; // Size = size of data + 4 bytes for internal id

                entries.Add(new ItemEntry<TItem>(entryId, item, (short)size));
            }

            return new ItemIndexDataPage<TItem>(header, entries);
        }

        /// <summary>
        /// Restores the data page from the specified data reader.
        /// </summary>
        /// <param name="dataReader">The binary reader to read the page data from. This will
        /// be located at the first byte after the page's header.</param>
        /// <param name="header">The header of the data page.</param>
        /// <returns>
        /// The restored page.
        /// </returns>
        public static IndexNodeDataPage RestoreIndexNodePage(this BinaryReader dataReader, IDataPageHeader header)
        {
            var count = header.EntryCount;
            var entries = new List<IndexNodeEntryBase>(header.EntryCount);

            for (var i = 0; i < count; i++)
            {
                var entryType = (IndexNodeEntryType)dataReader.ReadByte();
                var entryId = dataReader.ReadInt32();
                var referencedId = dataReader.ReadInt32();
                switch (entryType)
                {
                    case IndexNodeEntryType.ItemReference:
                        entries.Add(new ItemReferenceIndexNodeEntry(entryId, referencedId, dataReader.ReadInt32()));
                        break;

                    case IndexNodeEntryType.NodeReference:
                        entries.Add(new NodeReferenceIndexNodeEntry(entryId, referencedId, dataReader.ReadChar()));
                        break;

                    default:
                        throw new PersistenceException("Unexpected index node entry type encountered - this probably indicates file corruption");
                }
            }

            return new IndexNodeDataPage(header, entries);
        }

        /// <summary>
        /// Persists the data page, along with its header, to the specified data writer.
        /// </summary>
        /// <typeparam name="TItem">The type of the item in the index.</typeparam>
        /// <param name="dataWriter">The binary writer to write the page data to. This will be located
        /// at the position just after the persisted page's header.</param>
        /// <param name="page">The page to persist.</param>
        /// <param name="typePersistence">The type persistence implementation for TItem.</param>
        public static void PersistItemIndexPage<TItem>(this BinaryWriter dataWriter, ItemIndexDataPage<TItem> page, ITypePersistence<TItem> typePersistence)
        {
            System.Diagnostics.Debug.WriteLine("Writing page " + page.Header.PageNumber + " - " + page.Header.EntryCount);

            foreach (var entry in page.Entries)
            {
                dataWriter.Write(entry.Id);
                typePersistence.DataWriter(dataWriter, entry.Item);
            }
        }

        /// <summary>
        /// Persists the data page, along with its header, to the specified data writer.
        /// </summary>
        /// <param name="dataWriter">The binary writer to write the page data to. This will be located
        /// at the position just after the persisted page's header.</param>
        /// <param name="page">The page to persist.</param>
        public static void PersistItemNodeIndexDataPage(this BinaryWriter dataWriter, ItemNodeIndexDataPage page)
        {
            foreach (var entry in page.Entries)
            {
                dataWriter.Write(entry.Id);
                dataWriter.Write(entry.ReferencedId);
            }
        }

        /// <summary>
        /// Persists the data page, along with its header, to the specified data writer.
        /// </summary>
        /// <param name="dataWriter">The binary writer to write the page data to. This will be located
        /// at the position just after the persisted page's header.</param>
        /// <param name="page">The page to persist.</param>
        public static void PersistIndexNodeDataPage(this BinaryWriter dataWriter, IndexNodeDataPage page)
        {
            foreach (var entry in page.Entries)
            {
                dataWriter.Write((byte)entry.EntryType);
                dataWriter.Write(entry.Id);
                dataWriter.Write(entry.ReferencedId);
                if (entry.EntryType == IndexNodeEntryType.NodeReference)
                {
                    dataWriter.Write(((NodeReferenceIndexNodeEntry)entry).MatchedCharacter);
                }
                else if (entry.EntryType == IndexNodeEntryType.ItemReference)
                {
                    dataWriter.Write(((ItemReferenceIndexNodeEntry)entry).MatchPosition);
                }
            }
        }
    }
}
