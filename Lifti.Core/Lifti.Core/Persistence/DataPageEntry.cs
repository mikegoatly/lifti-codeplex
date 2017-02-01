// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    /// <summary>
    /// The base class for entries within a data page.
    /// </summary>
    public abstract class DataPageEntry : IDataPageEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPageEntry"/> class.
        /// </summary>
        /// <param name="id">The unique id of the entry.</param>
        /// <param name="size">The size of the entry, in bytes.</param>
        protected DataPageEntry(int id, short size)
        {
            this.Id = id;
            this.Size = size;
        }

        /// <summary>
        /// Gets the size of the item contained in the entry, in bytes.
        /// </summary>
        /// <value>The size of the item in the entry.</value>
        public short Size
        {
            get; }

        /// <summary>
        /// Gets the unique id for the entry.
        /// </summary>
        /// <value>The unique id for the entry.</value>
        public int Id
        {
            get; }
    }
}
