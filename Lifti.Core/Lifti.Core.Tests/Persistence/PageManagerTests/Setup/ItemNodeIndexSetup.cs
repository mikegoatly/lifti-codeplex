// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Information about the relationship between an item and the nodes it is associated to, setup as part of a test.
    /// </summary>
    public class ItemNodeIndexSetup
    {
        /// <summary>
        /// The chained list of indexed items.
        /// </summary>
        private readonly List<ItemNodeIndexSetup> items;

        /// <summary>
        /// The indexed item.
        /// </summary>
        private readonly int referencedId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeIndexSetup"/> class.
        /// </summary>
        /// <param name="itemId">The internal id of the item.</param>
        /// <param name="nodeId">The referenced node id.</param>
        public ItemNodeIndexSetup(int itemId, int nodeId)
            : this(itemId, nodeId, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemNodeIndexSetup"/> class.
        /// </summary>
        /// <param name="itemId">The internal id of the item.</param>
        /// <param name="nodeId">The referenced node id.</param>
        /// <param name="isRootReference">if set to <c>true</c> this instance will be responsible for managing the chain of indexed items.</param>
        public ItemNodeIndexSetup(int itemId, int nodeId, bool isRootReference)
        {
            this.Id = nodeId;
            this.referencedId = nodeId;
            if (isRootReference)
            {
                this.items = new List<ItemNodeIndexSetup> { this };
            }

            this.Size = 8;
        }

        /// <summary>
        /// Gets the size of the entry.
        /// </summary>
        public short Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the internal id of the item.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the number of indexed items.
        /// </summary>
        /// <value>The number of indexed items.</value>
        public int Count
        {
            get
            {
                return this.items.Count;
            }
        }

        /// <summary>
        /// Gets the chain of indexed items.
        /// </summary>
        /// <value>The indexed items.</value>
        public IEnumerable<ItemNodeIndexSetup> Items
        {
            get { return this.items; }
        }

        /// <summary>
        /// Adds another indexed item setup to this instance.
        /// </summary>
        /// <param name="itemId">The internal id of the item.</param>
        /// <param name="nodeId">The referenced node id.</param>
        /// <returns>This instance.</returns>
        public ItemNodeIndexSetup AndItem(int itemId, int nodeId)
        {
            this.items.Add(new ItemNodeIndexSetup(itemId, nodeId, false));
            return this;
        }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(this.Id);
            writer.Write(this.referencedId);
        }
    }
}
