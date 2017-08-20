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

    using NUnit.Framework;
    using System.Reflection;

    using Should;

    /// <summary>
    /// Integration tests for the persisted full text index.
    /// </summary>
    [TestFixture]
    public class IntegrationTests
    {
        private FileStream stream;
        private PersistedFullTextIndex<string> sut;

        private FileInfo file;

        [SetUp]
        public void SetUp()
        {
            this.file = new FileInfo("testindex.dat");
            this.stream = this.file.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite);
            this.stream.SetLength(0L);
            this.sut = new PersistedFullTextIndex<string>(this.stream);
        }

        [TearDown]
        public void CleanUp()
        {
            this.sut.Dispose();
            this.stream.Dispose();
            this.file.Delete();
        }

        /// <summary>
        /// Tests that an index containing strings can be persisted and restored successfully.
        /// </summary>
        [Test]
        public void PersistAndRestoreStringIndex()
        {
            this.sut.Index("Item 1", "The quick brown fox jumped over the lazy dog");

            this.stream.Position = 0L;
            using (var index = new PersistedFullTextIndex<string>(this.stream))
            {
                index.RootNode.Match('T').Match('H').Match('E').GetDirectItems().First().Positions.ShouldEqual(new[] { 0, 6 });
                index.Search("quick").Single().ShouldEqual("Item 1");
            }
        }

        /// <summary>
        /// Tests that removing the last item from an index should leave it empty.
        /// </summary>
        [Test]
        public void RemovingLastItemFromIndexShouldLeaveFileStoreEmpty()
        {
            this.sut.Index("Item 1", "The quick brown fox jumped over the lazy dog");

            this.sut.Remove("Item 1");

            this.sut.Search("quick").Count().ShouldEqual(0);
        }

        /// <summary>
        /// When removing an item that doesn't exist, no exception should be thrown.
        /// </summary>
        [Test]
        public void RemovingItemThatDoesntExistShouldntThrowException()
        {
            this.sut.Remove("Item 1");
        }

        /// <summary>
        /// Stress loads the index with lots of data, and attempts to reload it.
        /// </summary>
        [Test]
        public async Task StressLoadIndex()
        {
            this.sut.WordSplitter = new XmlWordSplitter(new StemmingWordSplitter());
            this.sut.SearchWordSplitter = new WordSplitter();

            foreach (var page in this.EnumeratePages())
            {
                this.sut.Index(page.Key, page.Value);
            }

            this.stream.Position = 0L;
            var index = new PersistedFullTextIndex<string>(this.stream);
            
            index.Count.ShouldEqual(200);

            // Read different parts of the index from 5 threads
            var barrier = new Barrier(5);

            await Task.WhenAll(
                Task.Run(() => { barrier.SignalAndWait(); index.Search("A").Count().ShouldEqual(200); }),
                Task.Run(() => { barrier.SignalAndWait(); index.Search("B").Count().ShouldEqual(200); }),
                Task.Run(() => { barrier.SignalAndWait(); index.Search("C").Count().ShouldEqual(200); }),
                Task.Run(() => { barrier.SignalAndWait(); index.Search("D").Count().ShouldEqual(200); }),
                Task.Run(() => { barrier.SignalAndWait(); index.Search("Z").Count().ShouldEqual(61); }));
        }

        /// <summary>
        /// Enumerates the Wikipedia pages in the compressed file.
        /// </summary>
        /// <returns>The pages contained within the compressed test file.</returns>
        private IEnumerable<KeyValuePair<string, string>> EnumeratePages()
        {
            using (var fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(ActionExtensions), "WikipediaPages.dat"))
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
