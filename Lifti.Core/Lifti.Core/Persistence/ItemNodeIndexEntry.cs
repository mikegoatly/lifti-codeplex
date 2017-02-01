// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Diagnostics;

    /// <summary>
    /// A data entry that relates an item in the index to a node that it is indexed against. This is the reverse of an
    /// <see cref="ItemReferenceIndexNodeEntry"/>
    /// </summary>
    [DebuggerDisplay("{Id} references {ReferencedId}")]
    public class ItemNodeIndexEntry : DataPageEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeIndexEntry"/> class.
        /// </summary>
        /// <param name="id">The internal id of the entry.</param>
        /// <param name="referencedId">The internal id of the referenced index node.</param>
        public ItemNodeIndexEntry(int id, int referencedId)
            : base(id, 8)
        {
            this.ReferencedId = referencedId;
        }

        /// <summary>
        /// Gets the internal id of the referenced index node.
        /// </summary>
        /// <value>The internal id of the referenced index node.</value>
        public int ReferencedId
        {
            get; }
    }
}
