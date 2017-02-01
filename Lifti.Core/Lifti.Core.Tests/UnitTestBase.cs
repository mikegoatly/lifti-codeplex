// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// The base class for unit tests - contains helper validation methods.
    /// </summary>
    public abstract class UnitTestBase
    {
        /// <summary>
        /// Asserts that the given action raises an argument null exception.
        /// </summary>
        /// <param name="action">The action to call.</param>
        /// <param name="argumentName">The name of the argument that should be reported as being null.</param>
        protected static void AssertRaisesArgumentNullException(Action action, string argumentName)
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

        /// <summary>
        /// Asserts that the given action raises the specified exception.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="action">The action to call.</param>
        /// <param name="expectedMessage">The expected message.</param>
        protected static void AssertRaisesException<TException>(Action action, string expectedMessage)
            where TException : Exception
        {
            var exceptionThrown = false;
            try
            {
                action();
            }
            catch (TException ex)
            {
                exceptionThrown = true;
                Assert.AreEqual(expectedMessage, ex.Message);
            }

            if (!exceptionThrown)
            {
                Assert.Fail("The expected " + typeof(TException).Name + " was not thrown");
            }
        }
    }
}
