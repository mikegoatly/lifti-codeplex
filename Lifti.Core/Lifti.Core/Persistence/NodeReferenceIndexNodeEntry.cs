// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// A data entry that describes a link between a serialized index node entry and another index node.
    /// </summary>
    [DebuggerDisplay("{Id}: Referenced node {ReferencedId} - {MatchedCharacter}")]
    public class NodeReferenceIndexNodeEntry : IndexNodeEntryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NodeReferenceIndexNodeEntry"/> class.
        /// </summary>
        /// <remarks>Size is 9 bytes plus the number of bytes required to serialize the character. 
        /// (9 bytes = 1 for the type, 4 for the internal id and 4 for the referenced id).</remarks>
        /// <param name="id">The internal id of the entry.</param>
        /// <param name="referenceId">The id of the index node that this entry refers to.</param>
        /// <param name="matchedCharacter">The matched character that connects this index node and the next.</param>
        public NodeReferenceIndexNodeEntry(int id, int referenceId, char matchedCharacter)
            : base(id, referenceId, (short)(9 + Encoding.UTF8.GetByteCount(new[] { matchedCharacter })))
        {
            this.MatchedCharacter = matchedCharacter;
        }

        /// <summary>
        /// Gets the type of the entry.
        /// </summary>
        /// <value>The type of the entry.</value>
        public override IndexNodeEntryType EntryType
        {
            get { return IndexNodeEntryType.NodeReference; }
        }

        /// <summary>
        /// Gets the matched character that connects this index node and the next.
        /// </summary>
        /// <value>The matched character.</value>
        public char MatchedCharacter
        {
            get; }
    }
}
