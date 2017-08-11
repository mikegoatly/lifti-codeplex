// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Diagnostics;

    /// <summary>
    /// A data entry that describes a link between a serialized index node entry and an indexed item.
    /// </summary>
    [DebuggerDisplay("{Id}: Referenced item {ReferencedId} - {MatchPosition}")]
    public class ItemReferenceIndexNodeEntry : IndexNodeEntryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ItemReferenceIndexNodeEntry"/> class.
        /// </summary>
        /// <param name="id">The internal id of the entry.</param>
        /// <param name="referenceId">The internal id of the indexed item that this entry refers to.</param>
        /// <param name="matchPosition">The position in the text for which the item's word was located at.</param>
        /// <remarks>
        /// Size is 13 bytes, 1 for the type, 4 for the internal id, 4 for the referenced id and 4 for the 
        /// positional information.
        /// </remarks>
        public ItemReferenceIndexNodeEntry(int id, int referenceId, int matchPosition)
            : base(id, referenceId, 13)
        {
            this.MatchPosition = matchPosition;
        }

        /// <summary>
        /// Gets the type of the entry.
        /// </summary>
        /// <value>The type of the entry.</value>
        public override IndexNodeEntryType EntryType => IndexNodeEntryType.ItemReference;

        /// <summary>
        /// Gets the position in the text for which the item's word was located at.
        /// </summary>
        /// <value>The position the item's word was located at.</value>
        public int MatchPosition
        {
            get; }
    }
}
