// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;

    using NUnit.Framework;

    /// <summary>
    /// The base class for unit tests - contains helper validation methods.
    /// </summary>
    public static class ActionExtensions
    {
        /// <summary>
        /// Asserts that the given action raises an argument null exception.
        /// </summary>
        /// <param name="test">The object used to anchor the method.</param>
        /// <param name="action">The action to call.</param>
        /// <param name="argumentName">The name of the argument that should be reported as being null.</param>
        public static void AssertRaisesArgumentNullException(this object test, Action action, string argumentName)
        {
            var exceptionThrown = false;
            try
            {
                action();
            }
            catch (ArgumentNullException ex)
            {
                exceptionThrown = true;
                if (ex.ParamName != argumentName)
                {
                    Assert.Fail("ArgumentNullException thrown, but parameter name was {0} - expecting {1}", ex.ParamName, argumentName);
                }
            }

            if (!exceptionThrown)
            {
                Assert.Fail("The expected ArgumentNullException was not thrown");
            }
        }
    }
}
