// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System.Collections.Generic;

    using Moq.Language.Flow;

    /// <summary>
    /// Extensions for Moq.
    /// </summary>
    public static class MoqExtensions
    {
        /// <summary>
        /// Enforces that a mocked setup returns the given items in sequence.
        /// </summary>
        /// <typeparam name="T">The type of the instance being tested.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="setup">The test setup.</param>
        /// <param name="results">The results to return, in order.</param>
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup, params TResult[] results) 
            where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
        }
    }
}
