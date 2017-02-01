// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods for <see cref="LinkedList{T}"/>.
    /// </summary>
    internal static class LinkedListExtensions
    {
        /// <summary>
        /// Adds the given value to the list when the predicate matches, or at the end of the list if no entry matches.
        /// </summary>
        /// <typeparam name="T">The type of item in the list.</typeparam>
        /// <param name="list">The list to insert into.</param>
        /// <param name="value">The value to insert.</param>
        /// <param name="predicate">The predicate to match.</param>
        public static void AddWhenOrLast<T>(this LinkedList<T> list, T value, Predicate<T> predicate)
        {
            var node = list.First;
            while (node != null)
            {
                if (predicate(node.Value))
                {
                    list.AddBefore(node, value);
                    return;
                }

                node = node.Next;
            }

            list.AddLast(value);
        }
    }
}
