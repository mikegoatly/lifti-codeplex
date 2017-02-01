// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.IO;

    using Lifti.Persistence;

    /// <summary>
    /// Information about an item reference setup as part of a test.
    /// </summary>
    public class ItemRefSetup : RefSetupBase
    {
        /// <summary>
        /// The word position.
        /// </summary>
        private int wordPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRefSetup"/> class.
        /// </summary>
        /// <param name="id">The internal id.</param>
        /// <param name="referencedId">The referenced internal item id.</param>
        /// <param name="wordPosition">The word position.</param>
        public ItemRefSetup(int id, int referencedId, int wordPosition)
            : this(id, referencedId, wordPosition, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemRefSetup"/> class.
        /// </summary>
        /// <param name="id">The internal id.</param>
        /// <param name="referencedId">The referenced internal item id.</param>
        /// <param name="wordPosition">The word position.</param>
        /// <param name="isRootReference">if set to <c>true</c> this instance will be responsible for managing the chain of indexed items.</param>
        public ItemRefSetup(int id, int referencedId, int wordPosition, bool isRootReference)
            : base(id, referencedId, isRootReference, 13)
        {
            this.wordPosition = wordPosition;
        }

        /// <summary>
        /// Writes this instance to the given binary writer instance.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        public override void WriteTo(BinaryWriter writer)
        {
            writer.Write((byte)IndexNodeEntryType.ItemReference);
            writer.Write(this.Id);
            writer.Write(this.ReferencedId);
            writer.Write(this.wordPosition);
        }
    }
}