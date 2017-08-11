// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;

    /// <summary>
    /// The comparer used to determine whether or not the items within a word match are equal.
    /// </summary>
    /// <typeparam name="TItem">The type of the item being indexed.</typeparam>
    public sealed class ItemWordMatchEqualityComparer<TItem> : IEqualityComparer<ItemWordMatch<TItem>>
    {
        /// <summary>
        /// The singleton instance of the comparer.
        /// </summary>
        private static readonly ItemWordMatchEqualityComparer<TItem> instance = new ItemWordMatchEqualityComparer<TItem>();

        /// <summary>
        /// Prevents a default instance of the <see cref="ItemWordMatchEqualityComparer&lt;TItem&gt;"/> class from being created.
        /// </summary>
        private ItemWordMatchEqualityComparer()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the comparer.
        /// </summary>
        /// <value>The singleton instance.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "This represents the singleton instance relevant to the specified generic implementation")]
        public static ItemWordMatchEqualityComparer<TItem> Instance => instance;

        /// <summary>
        /// Determines whether the items within two matches are equal.
        /// </summary>
        /// <param name="x">The first item match.</param>
        /// <param name="y">The second item match.</param>
        /// <returns><c>true</c> if the items within the matches are equal, otherwise <c>false</c>.</returns>
        public bool Equals(ItemWordMatch<TItem> x, ItemWordMatch<TItem> y)
        {
            return x.Item.Equals(y.Item);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <param name="obj">The object to get the hash code for.</param>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public int GetHashCode(ItemWordMatch<TItem> obj)
        {
            return obj.Item.GetHashCode();
        }
    } 
}
