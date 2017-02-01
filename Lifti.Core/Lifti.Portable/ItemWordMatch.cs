// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Properties;

    #endregion

    /// <summary>
    /// Stores location-based information about a word an item was indexed against.
    /// </summary>
    /// <typeparam name="TKey">The type of the key being indexed.</typeparam>

    public struct ItemWordMatch<TKey> : IEquatable<ItemWordMatch<TKey>>
    {
        /// <summary>
        /// The positions the word was matched in, if there were multiple matches.
        /// </summary>
        private readonly int[] positions;

        /// <summary>
        /// The indexed item.
        /// </summary>
        private readonly TKey item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemWordMatch&lt;TKey&gt;"/> struct.
        /// </summary>
        /// <param name="item">The indexed item.</param>
        /// <param name="positions">The list of positions the word was indexed at for the item. This must me in ascending order.</param>
        public ItemWordMatch(TKey item, int[] positions)
        {
            if (positions == null)
            {
                throw new ArgumentNullException(nameof(positions));
            }

            System.Diagnostics.Debug.Assert(positions.SequenceEqual(positions.OrderBy(i => i)), "Specified positions are not provided in order.");

            this.item = item;
            this.positions = positions;
        }

        /// <summary>
        /// Gets the item the word was matched for.
        /// </summary>
        /// <value>The indexed item.</value>
        public TKey Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ItemWordMatch&lt;TKey&gt;"/> is a successful match. An match is unsuccessful
        /// when it is not matched at any positions - this may occur when two matches are positionally unioned with no related
        /// positions.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success
        {
            get
            {
                return this.positions.Length > 0;
            }
        }

        /// <summary>
        /// Gets the word index positions that this match is relevant to.
        /// </summary>
        /// <value>The relevant word index positions.</value>
        public IEnumerable<int> Positions
        {
            get
            {
                return this.positions;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="itemA">The first item to check.</param>
        /// <param name="itemB">The second item to check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(ItemWordMatch<TKey> itemA, ItemWordMatch<TKey> itemB)
        {
            return itemA.Equals(itemB);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="itemA">The first item to check.</param>
        /// <param name="itemB">The second item to check.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(ItemWordMatch<TKey> itemA, ItemWordMatch<TKey> itemB)
        {
            return !itemA.Equals(itemB);
        }

        /// <summary>
        /// Compares this instance with the given instance and determines whether any of their positions
        /// are near to one of the others. If they are, a new instance for the matched item is returned containing
        /// only the position from the given instance that was near.
        /// </summary>
        /// <param name="nextMatch">The next match to compare with.</param>
        /// <param name="leftTolerance">The left tolerance.</param>
        /// <param name="rightTolerance">The right tolerance.</param>
        /// <returns>A new <see cref="ItemWordMatch&lt;TKey&gt;"/> with the positions that are near each other in this and the 
        /// specified instance.</returns>
        /// <example>
        /// If there are two matches:
        ///     <list>
        ///         <item>matchA - word positions 8, 44</item>
        ///         <item>matchB - word positions 7, 49, 102</item>
        ///     </list>
        /// Calling the code with the following parameters yields the following results.
        /// <code><![CDATA[item1.PositionalMatch(item2, 5, 5)]]></code>
        /// Returns: matchC - word positions 7, 49
        /// <code><![CDATA[item1.PositionalMatch(item2, 0, 5)]]></code>
        /// Returns: matchC - word positions 49
        /// <code><![CDATA[item1.PositionalMatch(item2, 5, 0)]]></code>
        /// Returns: matchC - word positions 7
        /// <code><![CDATA[item1.PositionalMatch(item2, 0, Int32.MaxValue)]]></code>
        /// Returns: matchC - word positions 49, 102
        /// <code><![CDATA[item1.PositionalMatch(item2, Int32.MaxValue, 0)]]></code>
        /// Returns: matchC - word positions 7
        /// <code><![CDATA[item1.PositionalMatch(item2, 0, 2)]]></code>
        /// Returns: matchC - word positions {none}
        /// </example>
        public ItemWordMatch<TKey> PositionalIntersect(ItemWordMatch<TKey> nextMatch, int leftTolerance, int rightTolerance)
        {
            if (!this.Equals(nextMatch))
            {
                throw new InvalidOperationException(Resources.UnableToPerformPositionalMatchBetweenDifferentItems);
            }

            var thisInstance = this;

            var matchingPositions = nextMatch.positions.Where(p => thisInstance.IsNear(p, leftTolerance, rightTolerance)).ToArray();
            return new ItemWordMatch<TKey>(nextMatch.Item, matchingPositions);
        }

        /// <summary>
        /// Determines whether the word was indexed near the specified position.
        /// </summary>
        /// <param name="position">The position to check at.</param>
        /// <param name="leftTolerance">The left tolerance to allow.</param>
        /// <param name="rightTolerance">The right tolerance to allow.</param>
        /// <returns>
        ///     <c>true</c> if the word was indexed at the specified position +/- the given tolerance; otherwise, <c>false</c>.
        /// </returns>
        public bool IsNear(int position, int leftTolerance, int rightTolerance)
        {
            if (this.positions.Length == 0)
            {
                return false;
            }

            var minPossible = rightTolerance == int.MaxValue ? int.MinValue : position - rightTolerance;
            var maxPossible = leftTolerance == int.MaxValue ? int.MaxValue : position + leftTolerance;

            // Simple out-of-bounds check for the entire array
            if (this.positions[0] > maxPossible || this.positions[this.positions.Length - 1] < minPossible)
            {
                return false;
            }

            int diff;
            foreach (var p in this.positions)
            {
                if (p > maxPossible)
                {
                    // As the array is sorted nothing after this element can be any closer to matching, so short-circuit now
                    break;
                }

                diff = position - p;
                if (diff <= rightTolerance && diff >= -leftTolerance)
                {
                    // This position is a match
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not the item in this instance and the specified item are equal.
        /// </summary>
        /// <param name="other">The other item.</param>
        /// <returns><c>true</c> if the items are equal, otherwise <c>false</c>.</returns>
        public bool Equals(ItemWordMatch<TKey> other)
        {
            return this.item.Equals(other.Item);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is ItemWordMatch<TKey> ? this.Equals((ItemWordMatch<TKey>)obj) : false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.item.GetHashCode();
        }
    }
}
