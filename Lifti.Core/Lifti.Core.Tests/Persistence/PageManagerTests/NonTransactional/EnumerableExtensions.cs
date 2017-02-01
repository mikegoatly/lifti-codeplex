// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.NonTransactional
{
    using System.Collections.Generic;
    using System.Linq;

    using Lifti.Persistence;

    /// <summary>
    /// Extensions for arrays.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the given sequence of page numbers equals the page numbers defined in the given
        /// sequence of page headers.
        /// </summary>
        /// <param name="pageNumbers">The page numbers.</param>
        /// <param name="headers">The headers to verify the page numbers for.</param>
        /// <returns><c>true</c> if the sequences are equal, otherwise <c>false</c>.</returns>
        public static bool SequenceEqual(this IEnumerable<int> pageNumbers, IEnumerable<IDataPageHeader> headers)
        {
            return pageNumbers.SequenceEqual(headers.Select(h => h.PageNumber));
        }
    }
}
