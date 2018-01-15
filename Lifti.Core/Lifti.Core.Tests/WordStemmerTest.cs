// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests
{
    using System;
    using System.IO;

    using Lifti.PorterStemmer;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the <see cref="Stemmer"/> class.
    /// </summary>
    [TestFixture]
    public class StemmerTest
    {
        /// <summary>
        /// Tests all the base test cases as specified in the files:
        /// http://snowball.tartarus.org/algorithms/porter/voc.txt and http://snowball.tartarus.org/algorithms/porter/output.txt
        /// </summary>
        [Test]
        public void StemWordTest()
        {
            var stemmer = new Stemmer();

            using (var stream = typeof(StemmerTest).Assembly.GetManifestResourceStream("Lifti.Tests.StemmerTestCases.txt"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    string[] testCase;
                    var space = new[] { ' ' };
                    while ((line = reader.ReadLine()) != null)
                    {
                        testCase = line.Split(space, StringSplitOptions.RemoveEmptyEntries);
                        if (testCase.Length != 2)
                        {
                            throw new Exception("Expected an array of two - word, stemmed word");
                        }

                        Assert.AreEqual(testCase[1], stemmer.Stem(testCase[0]), "Stemming {0} - expected {1}", testCase[0], testCase[1]);
                    }
                }
            }
        }
    }
}
