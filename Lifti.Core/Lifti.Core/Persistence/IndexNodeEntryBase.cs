// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// Describes whether an entry against an index node points towards another node or an indexed item.
    /// </summary>
    public enum IndexNodeEntryType
    {
        /// <summary>
        /// The entry type refers to an item that was indexed against the node. The id is an internal id 
        /// pointing to an item in the item index.
        /// </summary>
        ItemReference,

        /// <summary>
        /// The entry type refers to another node
        /// </summary>
        NodeReference
    }

    /// <summary>
    /// A data entry that relates a node in the text index.
    /// </summary>
    public abstract class IndexNodeEntryBase : DataPageEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNodeEntryBase"/> class.
        /// </summary>
        /// <param name="id">The internal id of the entry.</param>
        /// <param name="referenceId">The id that this entry refers to. This can be either an indexed item's internal id
        /// or a index node's internal id.</param>
        /// <param name="size">The size of the data contained in the data entry, when serialized.</param>
        protected IndexNodeEntryBase(int id, int referenceId, short size)
            : base(id, size)
        {
            this.ReferencedId = referenceId;
        }

        /// <summary>
        /// Gets the type of the entry.
        /// </summary>
        /// <value>The type of the entry.</value>
        public abstract IndexNodeEntryType EntryType
        {
            get;
        }

        /// <summary>
        /// Gets the referenced id.
        /// </summary>
        /// <value>The referenced id.</value>
        public int ReferencedId
        {
            get;
            private set;
        }
    }
}
