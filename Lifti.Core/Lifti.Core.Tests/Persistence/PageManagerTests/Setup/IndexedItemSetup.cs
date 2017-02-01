// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Information about an indexed item setup as part of a test.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    public class IndexedItemSetup<TItem>
    {
        /// <summary>
        /// The indexed item.
        /// </summary>
        private readonly TItem item;

        /// <summary>
        /// The chained list of indexed items.
        /// </summary>
        private readonly List<IndexedItemSetup<TItem>> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedItemSetup&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="id">The internal id of the item.</param>
        /// <param name="item">The indexed item.</param>
        public IndexedItemSetup(int id, TItem item)
            : this(id, item, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexedItemSetup&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="id">The internal id of the item.</param>
        /// <param name="item">The indexed item.</param>
        /// <param name="isRootReference">if set to <c>true</c> this instance will be responsible for managing the chain of indexed items.</param>
        public IndexedItemSetup(int id, TItem item, bool isRootReference)
        {
            this.Id = id;
            this.item = item;
            if (isRootReference)
            {
                this.items = new List<IndexedItemSetup<TItem>> { this };
            }

            this.Size = 4;
            switch (typeof(TItem).Name)
            {
                case "Int32":
                    this.Size += 4;
                    break;
                default:
                    throw new Exception("Unsupported test item type");
            }
        }

        /// <summary>
        /// Gets the size of the entry.
        /// </summary>
        public short Size
        {
            get; }

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
        public IEnumerable<IndexedItemSetup<TItem>> Items
        {
            get { return this.items; }
        }

        /// <summary>
        /// Adds another indexed item setup to this instance.
        /// </summary>
        /// <param name="id">The internal id of the item.</param>
        /// <param name="item">The indexed item.</param>
        /// <returns>This instance.</returns>
        public IndexedItemSetup<TItem> AndItem(int id, TItem item)
        {
            this.items.Add(new IndexedItemSetup<TItem>(id, item, false));
            return this;
        }

        /// <summary>
        /// Writes this instance to the given writer.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write to.</param>
        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(this.Id);
            switch (typeof(TItem).Name)
            {
                case "Int32":
                    ((dynamic)writer).Write(this.item);
                    break;
                default:
                    throw new Exception("Unsupported test item type");
            }
        }
    }
}