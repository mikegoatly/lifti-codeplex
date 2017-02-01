// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests for the <see cref="ItemWordMatch&lt;TItem&gt;"/> struct.
    /// </summary>
    [TestClass]
    public class ItemWordMatchTests : UnitTestBase
    {
        /// <summary>
        /// Tests that the PositionalIntersect method returns the expected results for
        /// known test cases.
        /// </summary>
        [TestMethod]
        public void PositionalIntersectShouldReturnCorrectResults()
        {
            var matchA = new ItemWordMatch<string>("Test", new[] { 8, 44 });
            var matchB = new ItemWordMatch<string>("Test", new[] { 7, 49, 102 });

            var tests = new[]
                        {
                            new { Left = 5, Right = 5, Expected = new[] { 7, 49 } },
                            new { Left = 0, Right = 5, Expected = new[] { 49 } },
                            new { Left = 5, Right = 0, Expected = new[] { 7 } },
                            new { Left = 0, Right = int.MaxValue, Expected = new[] { 49, 102 } },
                            new { Left = int.MaxValue, Right = 0, Expected = new[] { 7 } },
                            new { Left = 0, Right = 3, Expected = new int[0] }
                        };

            foreach (var test in tests)
            {
                var matchC = matchA.PositionalIntersect(matchB, test.Left, test.Right);
                Assert.AreEqual("Test", matchC.Item);
                Assert.IsTrue(matchC.Positions.SequenceEqual(test.Expected));
                Assert.AreEqual(test.Expected.Length > 0, matchC.Success);
            }
        }

        /// <summary>
        /// Verifies that when a positional match between matches for different items is attempted,
        /// an exception is thrown.
        /// </summary>
        [TestMethod]
        public void PositionalIntersectBetweenDifferentItemsShouldRaiseException()
        {
            AssertRaisesException<InvalidOperationException>(
                () => new ItemWordMatch<string>("A", new int[0]).PositionalIntersect(new ItemWordMatch<string>("B", new int[0]), 1, 1),
                "Internal error - Unable to perform positional match between item matches representing different items.");
        }

        /// <summary>
        /// Verifies that constructing an item word match with null positions raises an exception.
        /// </summary>
        [TestMethod]
        public void ConstructingItemWordMatchWithNullPositionsRaisesException()
        {
            AssertRaisesArgumentNullException(() => new ItemWordMatch<string>("Test", null), "positions");
        }

        /// <summary>
        /// Simple tests for the IsNear method with different parameters.
        /// </summary>
        [TestMethod]
        public void IsNearTests()
        {
            var tests = new[]
                            {
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 4, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 5, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 17, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 18, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 24, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 25, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 37, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 38, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 54, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 55, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 67, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = true },
                                new { WordPositions = new[] { 10, 30, 60 }, Test = 68, LeftTolerance = 5, RightTolerance = 7, ExpectedResult = false },
                                new { WordPositions = new[] { 1000000 }, Test = 2, LeftTolerance = int.MaxValue, RightTolerance = 0, ExpectedResult = true },
                                new { WordPositions = new[] { 1000000 }, Test = 2, LeftTolerance = 0, RightTolerance = int.MaxValue, ExpectedResult = false },
                                new { WordPositions = new[] { 1000000 }, Test = 34000000, LeftTolerance = int.MaxValue, RightTolerance = 0, ExpectedResult = false },
                                new { WordPositions = new[] { 1000000 }, Test = 34000000, LeftTolerance = 0, RightTolerance = int.MaxValue, ExpectedResult = true },
                                new { WordPositions = new int[0], Test = 56, LeftTolerance = int.MaxValue, RightTolerance = int.MaxValue, ExpectedResult = false }
                            };

            foreach (var test in tests)
            {
                var word = new ItemWordMatch<string>(string.Empty, test.WordPositions);
                Assert.AreEqual(test.ExpectedResult, word.IsNear(test.Test, test.LeftTolerance, test.RightTolerance));
            }
        }

        /// <summary>
        /// Tests that calling the Equals(object) method passing in null returns false.
        /// </summary>
        [TestMethod]
        public void ObjectEqualsNullReturnsFalse()
        {
            Assert.IsFalse(new ItemWordMatch<string>("Test", new int[0]).Equals(null));
        }

        /// <summary>
        /// Tests that calling the Equals(object) method passing in varying values returns the correct results.
        /// </summary>
        [TestMethod]
        public void ObjectEqualsValueReturnsTrueWhenAppropriate()
        {
            var a = new ItemWordMatch<string>("Test", new int[0]);
            var b = new ItemWordMatch<string>("Test", new[] { 1, 2 });
            var c = new ItemWordMatch<string>("Test2", new[] { 2, 3 });

            Assert.IsTrue(a.Equals((object)a));
            Assert.IsTrue(a.Equals((object)b));
            Assert.IsFalse(a.Equals((object)c));
        }

        /// <summary>
        /// Tests the equality operator.
        /// </summary>
        [TestMethod]
        public void TestEqualityOperator()
        {
            var a = new ItemWordMatch<string>("Test", new int[0]);
            var b = new ItemWordMatch<string>("Test", new[] { 1, 2 });
            var c = new ItemWordMatch<string>("Test2", new[] { 2, 3 });

            Assert.IsTrue(a == b);
            Assert.IsFalse(a == c);
            Assert.IsFalse(b == c);
        }

        /// <summary>
        /// Tests the inequality operator.
        /// </summary>
        [TestMethod]
        public void TestInequalityOperator()
        {
            var a = new ItemWordMatch<string>("Test", new int[0]);
            var b = new ItemWordMatch<string>("Test", new[] { 1, 2 });
            var c = new ItemWordMatch<string>("Test2", new[] { 2, 3 });

            Assert.IsFalse(a != b);
            Assert.IsTrue(a != c);
            Assert.IsTrue(b != c);
        }
    }
}
