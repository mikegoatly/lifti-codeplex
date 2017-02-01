// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.IO;
    using System.Text;

    using Lifti.Persistence;

    /// <summary>
    /// Information about a node reference setup as part of a test.
    /// </summary>
    public class NodeRefSetup : RefSetupBase
    {
        /// <summary>
        /// The character associated to the node reference.
        /// </summary>
        private readonly char character;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeRefSetup"/> class.
        /// </summary>
        /// <param name="id">The internal id.</param>
        /// <param name="referencedNodeId">The referenced node id.</param>
        /// <param name="character">The character associated to the node link.</param>
        public NodeRefSetup(int id, int referencedNodeId, char character)
            : this(id, referencedNodeId, character, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeRefSetup"/> class.
        /// </summary>
        /// <param name="id">The internal id.</param>
        /// <param name="referencedNodeId">The referenced node id.</param>
        /// <param name="character">The character associated to the node link.</param>
        /// <param name="isRootReference">if set to <c>true</c> this instance will be responsible for managing the chain of indexed items.</param>
        public NodeRefSetup(int id, int referencedNodeId, char character, bool isRootReference)
            : base(id, referencedNodeId, isRootReference, (short)(9 + Encoding.UTF8.GetByteCount(new[] { character })))
        {
            this.character = character;
        }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte)IndexNodeEntryType.NodeReference);
            writer.Write(this.Id);
            writer.Write(this.ReferencedId);
            writer.Write(this.character);
        }
    }
}