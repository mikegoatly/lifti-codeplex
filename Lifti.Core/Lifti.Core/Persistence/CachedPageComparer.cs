// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System.Collections.Generic;

    /// <summary>
    /// The default comparer for cached page instances.
    /// </summary>
    internal class CachedPageComparer : IComparer<CachedPage>
    {
        /// <summary>
        /// Compares two instances of <see cref="CachedPage"/>. Priority is given to the last access date. Ties on the last access
        /// are resolved by the number of times the page has been accessed. If this ties, then the page number wins. This last
        /// step may seem counter intuitive, but items in a sorted list must be unique.
        /// </summary>
        /// <param name="x">The first <see cref="CachedPage"/> to compare.</param>
        /// <param name="y">The second <see cref="CachedPage"/> to compare.</param>
        /// <returns>
        /// Less than zero: <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero: <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero: <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(CachedPage x, CachedPage y)
        {
            var result = x.LastAccess.CompareTo(y.LastAccess);
            if (result == 0)
            {
                result = x.AccessCount.CompareTo(y.AccessCount);

                if (result == 0)
                {
                    result = x.Page.Header.PageNumber.CompareTo(y.Page.Header.PageNumber);
                }
            }

            return result;
        }
    }
}
