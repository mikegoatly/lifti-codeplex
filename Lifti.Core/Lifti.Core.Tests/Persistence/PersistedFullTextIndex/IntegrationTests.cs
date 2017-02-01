// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PersistedFullTextIndex
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Integration tests for the persisted full text index.
    /// </summary>
    [TestClass, DeploymentItem("WikipediaPages.dat")]
    public class IntegrationTests
    {
        /// <summary>
        /// The path to the text full text index.
        /// </summary>
        private string path;

        /// <summary>
        /// Cleans up before and after each test.
        /// </summary>
        [TestCleanup]
        [TestInitialize]
        public void TestCleanup()
        {
            this.path = $"testindex{Guid.NewGuid()}.dat";

            if (File.Exists(this.path))
            {
                File.Delete(this.path);
            }

            if (File.Exists(this.path + ".txlog"))
            {
                File.Delete(this.path + ".txlog");
            }
        }

        /// <summary>
        /// Tests that an index containing strings can be persisted and restored successfully.
        /// </summary>
        [TestMethod]
        public void PersistAndRestoreStringIndex()
        {
            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                index.Index("Item 1", "The quick brown fox jumped over the lazy dog");
            }

            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                Assert.IsTrue((new[] { 0, 6 }).SequenceEqual(index.RootNode.Match('T').Match('H').Match('E').GetDirectItems().First().Positions));
                Assert.AreEqual("Item 1", index.Search("quick").Single());
            }
        }

        /// <summary>
        /// Tests that removing the last item from an index should leave it empty.
        /// </summary>
        [TestMethod]
        public void RemovingLastItemFromIndexShouldLeaveFileStoreEmpty()
        {
            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                index.Index("Item 1", "The quick brown fox jumped over the lazy dog");
            }

            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                index.Remove("Item 1");
            }

            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                Assert.AreEqual(0, index.Search("quick").Count());
            }
        }

        /// <summary>
        /// When removing an item that doesn't exist, no exception should be thrown.
        /// </summary>
        [TestMethod]
        public void RemovingItemThatDoesntExistShouldntThrowException()
        {
            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                index.Remove("Item 1");
            }
        }

        /// <summary>
        /// Stress loads the index with lots of data, and attempts to reload it.
        /// </summary>
        [TestMethod]
        public void StressLoadIndex()
        {
            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                index.WordSplitter = new XmlWordSplitter(new StemmingWordSplitter());

                foreach (var page in this.EnumeratePages())
                {
                    index.Index(page.Key, page.Value);
                }
            }

            // Read different parts of the index from 3 threads
            using (var index = new PersistedFullTextIndex<string>(this.path))
            {
                Assert.IsTrue(index.Count == 200);

                var barrier = new Barrier(5);

                Parallel.Invoke(
                    new ParallelOptions { MaxDegreeOfParallelism = 5 }, 
                    () => { barrier.SignalAndWait(); Assert.AreEqual(200, index.Search("A").Count()); },
                    () => { barrier.SignalAndWait(); Assert.AreEqual(200, index.Search("B").Count()); },
                    () => { barrier.SignalAndWait(); Assert.AreEqual(200, index.Search("C").Count()); },
                    () => { barrier.SignalAndWait(); Assert.AreEqual(200, index.Search("D").Count()); },
                    () => { barrier.SignalAndWait(); Assert.AreEqual(61, index.Search("Z").Count()); });
            }
        }

        /// <summary>
        /// Enumerates the Wikipedia pages in the compressed file.
        /// </summary>
        /// <returns>The pages contained within the compressed test file.</returns>
        private IEnumerable<KeyValuePair<string, string>> EnumeratePages()
        {
            using (var fileStream = new FileStream("WikipediaPages.dat", FileMode.Open, FileAccess.Read))
            {
                using (var zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    using (var reader = new BinaryReader(zipStream))
                    {
                        for (var i = 0; i < 200; i++)
                        {
                            var fileName = reader.ReadString();
                            var contents = reader.ReadString();

                            yield return new KeyValuePair<string, string>("Originally downloaded from Wikipedia - http://en.wikipedia.com/wiki/pages/" + fileName, contents);
                        }
                    }
                }
            }
        }
    }
}
