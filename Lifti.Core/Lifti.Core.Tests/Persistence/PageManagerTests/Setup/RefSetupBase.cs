// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The base class for reference setups - both item references and node references.
    /// </summary>
    public abstract class RefSetupBase
    {
        /// <summary>
        /// The chained list of item and node references.
        /// </summary>
        private readonly List<RefSetupBase> references;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefSetupBase"/> class.
        /// </summary>
        /// <param name="id">The internal id.</param>
        /// <param name="referencedId">The referenced internal item id.</param>
        /// <param name="isRootReference">if set to <c>true</c> this instance will be responsible for managing the chain of indexed items.</param>
        /// <param name="size">The size of the entry.</param>
        protected RefSetupBase(int id, int referencedId, bool isRootReference, short size)
        {
            this.Id = id;
            this.ReferencedId = referencedId;
            if (isRootReference)
            {
                this.references = new List<RefSetupBase> { this };
            }

            this.Size = size;
        }

        /// <summary>
        /// Gets the size of the item.
        /// </summary>
        public short Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the internal id of the referencing item.
        /// </summary>
        public int Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the internal id of the referenced item.
        /// </summary>
        public int ReferencedId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the number of referenced items.
        /// </summary>
        /// <value>The number of referenced items.</value>
        public int Count
        {
            get { return this.references.Count; }
        }

        /// <summary>
        /// Gets the referenced items.
        /// </summary>
        /// <value>The referenced items.</value>
        public IEnumerable<RefSetupBase> References
        {
            get { return this.references; }
        }

        /// <summary>
        /// Chains a new node reference setup to this instance.
        /// </summary>
        /// <param name="id">The internal id of the referencing node.</param>
        /// <param name="referencedNodeId">The referenced node id.</param>
        /// <param name="character">The character the node reference represents.</param>
        /// <returns>This instance.</returns>
        public RefSetupBase AndNodeRef(int id, int referencedNodeId, char character)
        {
            this.references.Add(new NodeRefSetup(id, referencedNodeId, character, false));
            return this;
        }

        /// <summary>
        /// Chains a new item reference setup to this instance.
        /// </summary>
        /// <param name="id">The internal id of the referencing node.</param>
        /// <param name="itemId">The referenced item id.</param>
        /// <param name="wordPosition">The word position.</param>
        /// <returns>
        /// This instance.
        /// </returns>
        public RefSetupBase AndItemRef(int id, int itemId, int wordPosition)
        {
            this.references.Add(new ItemRefSetup(id, itemId, wordPosition, false));
            return this;
        }

        /// <summary>
        /// Writes this instance to the given binary writer instance.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        public abstract void WriteTo(BinaryWriter writer);
    }
}