// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Tests.Persistence.PageManagerTests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Lifti.Persistence.IO;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    /// <summary>
    /// A test helper class responsible for setting up a mocked IO manager.
    /// </summary>
    public class MockDataFileManagerSetup
    {
        /// <summary>
        /// Whether this instance is mocking a new file.
        /// </summary>
        private readonly bool newFile;

        /// <summary>
        /// The list of page setup classes that define the initial state of the pages that will be loaded from the IO manager.
        /// </summary>
        private readonly List<PageSetup> pageSetups = new List<PageSetup>();

        /// <summary>
        /// The file header data.
        /// </summary>
        private byte[] fileHeaderData;

        /// <summary>
        /// The page data.
        /// </summary>
        private List<byte[]> pageData;

        /// <summary>
        /// The page manager header data.
        /// </summary>
        private byte[] pageManagerHeaderData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockDataFileManagerSetup"/> class.
        /// </summary>
        /// <param name="newFile">Whether this instance is mocking a new file.</param>
        public MockDataFileManagerSetup(bool newFile)
        {
            this.newFile = newFile;
            this.Mock = new Mock<IDataFileManager>(MockBehavior.Strict);

            this.Mock.Setup(m => m.ShrinkStream(It.IsAny<int>())).Callback<int>(this.ShrinkPages);
            this.Mock.Setup(m => m.ExtendStream(It.IsAny<int>())).Callback<int>(this.ExtendPages);

            this.Mock.SetupGet(m => m.IsNewFile).Returns(newFile);
        }

        /// <summary>
        /// Gets the mocked <see cref="IDataFileManager"/> instance.
        /// </summary>
        /// <value>The mocked <see cref="IDataFileManager"/> instance.</value>
        public Mock<IDataFileManager> Mock
        {
            get; }

        /// <summary>
        /// Gets the file header data.
        /// </summary>
        /// <value>The file header data.</value>
        public MemoryStream FileHeader
        {
            get { return new MemoryStream(this.fileHeaderData); }
        }

        /// <summary>
        /// Gets the page manager header data.
        /// </summary>
        /// <value>The page manager header data.</value>
        public MemoryStream PageManagerHeader
        {
            get { return new MemoryStream(this.pageManagerHeaderData); }
        }

        /// <summary>
        /// Gets the total size of the mocked data.
        /// </summary>
        /// <value>The total size of the mocked data.</value>
        public int TotalMockedSize
        {
            get
            {
                return this.fileHeaderData.Length + this.pageManagerHeaderData.Length + (this.pageData.Count * Data.PageSize);
            }
        }

        /// <summary>
        /// Gets the list of page data.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>The list of page data.</returns>
        public MemoryStream GetPageData(int pageNumber)
        {
            return new MemoryStream(this.pageData[pageNumber]);
        }

        /// <summary>
        /// Adds a node index page to the setup.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="references">The references held within the page.</param>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup IndexNodePage(int pageNumber, int? previousPage, int? nextPage, RefSetupBase references)
        {
            var setup = new IndexNodePageSetup(pageNumber, previousPage, nextPage, references);
            this.pageSetups.Add(setup);
            return this;
        }

        /// <summary>
        /// Adds an item page to the setup.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="indexedItems">The indexed items held within the page.</param>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup ItemPage<TItem>(int pageNumber, int? previousPage, int? nextPage, IndexedItemSetup<TItem> indexedItems)
        {
            var setup = new ItemIndexPageSetup<TItem>(pageNumber, previousPage, nextPage, indexedItems);
            this.pageSetups.Add(setup);
            return this;
        }

        /// <summary>
        /// Automatically creates an item node index page that contains the reverse lookups for any already added items indexed
        /// against a node.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup AutoCreateItemNodeIndexPages(int pageNumber)
        {
            ItemNodeIndexSetup itemNodeIndexes = null;
            foreach (var reference in this.pageSetups
                .OfType<IndexNodePageSetup>()
                .Where(p => p.ReferencedItems != null)
                .SelectMany(p => p.ReferencedItems.References.OfType<ItemRefSetup>()))
            {
                if (itemNodeIndexes == null)
                {
                    itemNodeIndexes = new ItemNodeIndexSetup(reference.ReferencedId, reference.Id);
                }
                else
                {
                    itemNodeIndexes.AndItem(reference.ReferencedId, reference.Id);
                }
            }

            var setup = new ItemNodeIndexPageSetup(pageNumber, null, null, itemNodeIndexes);
            this.pageSetups.Add(setup);
            return this;
        }

        /// <summary>
        /// Adds an item node index page to the setup.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="previousPage">The previous page number.</param>
        /// <param name="nextPage">The next page number.</param>
        /// <param name="itemNodeIndexes">The items held within the page.</param>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup ItemNodeIndexPage(int pageNumber, int? previousPage, int? nextPage, ItemNodeIndexSetup itemNodeIndexes)
        {
            var setup = new ItemNodeIndexPageSetup(pageNumber, previousPage, nextPage, itemNodeIndexes);
            this.pageSetups.Add(setup);
            return this;
        }

        /// <summary>
        /// Adds an empty page to the setup.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup EmptyPage(int pageNumber)
        {
            var setup = new PageSetup(pageNumber, null, null);
            this.pageSetups.Add(setup);
            return this;
        }

        /// <summary>
        /// Verifies that the basic expectations of the mocked IO manager have been executed.
        /// This will verify different things depending on whether the IO manager was processing a new file.
        /// </summary>
        public void VerifyBasics()
        {
            if (this.newFile)
            {
                this.Mock.Verify(m => m.GetDataWriter(0, Data.HeaderSize), Times.Exactly(1));
                this.Mock.Verify(m => m.GetDataWriter(Data.HeaderSize, Data.PageManagerHeaderSize), Times.Exactly(1));
                this.Mock.Verify(m => m.ExtendStream((Data.PageSize * 4) + Data.HeaderSize + Data.PageManagerHeaderSize), Times.Exactly(1));

                foreach (var pageSetup in this.pageSetups)
                {
                    var pageOffset = Data.HeaderSize + Data.PageManagerHeaderSize + (Data.PageSize * pageSetup.PageNumber);
                    this.Mock.Verify(m => m.GetDataWriter(pageOffset, Data.PageSize), Times.AtLeastOnce());
                }
            }
            else
            {
                this.Mock.Verify(m => m.GetDataReader(0, Data.HeaderSize), Times.Exactly(1));
                this.Mock.Verify(m => m.GetDataReader(Data.HeaderSize, Data.PageManagerHeaderSize), Times.Exactly(1));
                this.Mock.Verify(m => m.GetDataWriter(0, Data.HeaderSize), Times.Never());

                foreach (var pageSetup in this.pageSetups)
                {
                    var pageOffset = Data.HeaderSize + Data.PageManagerHeaderSize + (Data.PageSize * pageSetup.PageNumber);
                    this.Mock.Verify(m => m.GetDataReader(pageOffset, Data.PageSize), Times.AtLeastOnce());
                }
            }
        }

        /// <summary>
        /// Verifies that the page manager header was written a number of times.
        /// </summary>
        /// <param name="times">The number of times the header should have been written.</param>
        public void VerifyPageManagerHeaderWritten(Times times)
        {
            this.Mock.Verify(m => m.GetDataWriter(Data.HeaderSize, Data.PageManagerHeaderSize), times);
        }

        /// <summary>
        /// Verifies that the data was never extended.
        /// </summary>
        public void VerifyNeverExtended()
        {
            this.Mock.Verify(m => m.ExtendStream(It.IsAny<int>()), Times.Never());
        }

        /// <summary>
        /// Prepares this instance, setting up all the expected behaviours given the various setups applied to it.
        /// </summary>
        /// <returns>This instance.</returns>
        public MockDataFileManagerSetup Prepare()
        {
            if (!this.pageSetups.OfType<ItemNodeIndexPageSetup>().Any() && this.pageSetups.Any())
            {
                this.AutoCreateItemNodeIndexPages(this.pageSetups.Max(p => p.PageNumber) + 1);
            }

            this.PrepareHeaderData();
            this.PreparePageHeaderData();
            this.PreparePageData();

            return this;
        }

        /// <summary>
        /// Verifies that the specified page is read the expected number of times.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="times">The times the page should have been read.</param>
        public void VerifyPageRead(int pageNumber, Times times)
        {
            this.Mock.Verify(m => m.GetDataReader(Data.HeaderSize + Data.PageManagerHeaderSize + (pageNumber * Data.PageSize), Data.PageSize), times);
        }

        /// <summary>
        /// Verifies that the specified page is read the expected number of times.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="times">The times the page should have been read.</param>
        public void VerifyPageBodyRead(int pageNumber, Times times)
        {
            this.Mock.Verify(m => m.GetDataReader(Data.PageHeaderSize + Data.HeaderSize + Data.PageManagerHeaderSize + (pageNumber * Data.PageSize), Data.PageSize - Data.PageHeaderSize), times);
        }

        /// <summary>
        /// Verifies that the specified page is written the expected number of times.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="times">The times the page should have been written.</param>
        public void VerifyPageWritten(int pageNumber, Times times)
        {
            this.Mock.Verify(m => m.GetDataWriter(Data.HeaderSize + Data.PageManagerHeaderSize + (pageNumber * Data.PageSize), Data.PageSize), times);
        }

        /// <summary>
        /// Verifies that the specified page is written the expected number of times.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="times">The times the page should have been written.</param>
        public void VerifyPageBodyWritten(int pageNumber, Times times)
        {
            this.Mock.Verify(m => m.GetDataWriter(Data.PageHeaderSize + Data.HeaderSize + Data.PageManagerHeaderSize + (pageNumber * Data.PageSize), Data.PageSize - Data.PageHeaderSize), times);
        }

        /// <summary>
        /// Verifies that the specified page header is written the expected number of times.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="times">The times the page should have been written.</param>
        public void VerifyPageHeaderWritten(int pageNumber, Times times)
        {
            this.Mock.Verify(m => m.GetDataWriter(Data.HeaderSize + Data.PageManagerHeaderSize + (pageNumber * Data.PageSize), Data.PageSize), times);
        }

        /// <summary>
        /// Creates a binary reader for the given data.
        /// </summary>
        /// <param name="data">The data to read.</param>
        /// <returns>
        /// A delegate capable of returning the binary reader.
        /// </returns>
        private static Func<BinaryReader> CreateMemoryReader(byte[] data)
        {
            return () =>
                {
                    return new BinaryReader(new MemoryStream(data));
                };
        }

        /// <summary>
        /// Gets the modulus of the given offset and the size of the page.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The modulus value.</returns>
        private static int PageMod(int offset)
        {
            return (offset - Data.HeaderSize - Data.PageManagerHeaderSize) % Data.PageSize;
        }

        /// <summary>
        /// Creates a binary writer for the given data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <returns>
        /// A delegate capable of returning the binary writer.
        /// </returns>
        private static Func<BinaryWriter> CreateMemoryWriter(byte[] data)
        {
            return () =>
                {
                    return new BinaryWriter(new MemoryStream(data));
                };
        }

        /// <summary>
        /// Creates a binary reader for the page data representing the requested data range, resetting the stream to the start.
        /// </summary>
        /// <returns>/// A delegate capable of returning the binary reader.</returns>
        private Func<int, int, BinaryReader> CreatePageMemoryReader()
        {
            return (offset, length) =>
                {
                    var mod = PageMod(offset);
                    var pageNumber = this.GetPageNumberForOffset(offset, length, mod);

                    var stream = this.GetPageData(pageNumber);
                    stream.Position = mod;
                    return new BinaryReader(stream);
                };
        }

        /// <summary>
        /// Extends the pages.
        /// </summary>
        /// <param name="newMemorySize">The new size of the memory.</param>
        private void ExtendPages(int newMemorySize)
        {
            var totalMockedSize = this.TotalMockedSize;
            if (newMemorySize > totalMockedSize)
            {
                // Create some new empty pages
                if ((newMemorySize - totalMockedSize) % Data.PageSize != 0)
                {
                    Assert.Fail("Only ever expect to extend by multiples of PageSize...");
                }

                this.pageData.AddRange(
                    Enumerable.Range(0, (newMemorySize - totalMockedSize) / Data.PageSize)
                        .Select(i => new byte[Data.PageSize]));
            }
        }

        /// <summary>
        /// Shrinks the pages.
        /// </summary>
        /// <param name="newMemorySize">The new size of the memory.</param>
        private void ShrinkPages(int newMemorySize)
        {
            var totalMockedSize = this.TotalMockedSize;
            if (totalMockedSize > newMemorySize)
            {
                // Create some new empty pages
                if ((totalMockedSize - newMemorySize) % Data.PageSize != 0)
                {
                    Assert.Fail("Only ever expect to shrink by multiples of PageSize...");
                }

                var newPageCount = (totalMockedSize - newMemorySize) / Data.PageSize;
                this.pageData.RemoveRange(newPageCount, this.pageData.Count - newPageCount);
            }
        }

        /// <summary>
        /// Gets the page number for the given offset.
        /// </summary>
        /// <param name="offset">The offset of the requested data.</param>
        /// <param name="length">The length of the requested data.</param>
        /// <param name="mod">The memory modulus for the offset.</param>
        /// <returns>
        /// The validated page number.
        /// </returns>
        private int GetPageNumberForOffset(int offset, int length, int mod)
        {
            if (length != Data.PageSize && length != Data.PageSize - Data.PageHeaderSize)
            {
                Assert.Fail("Unexpected data length requested");   
            }

            // Page data can be requested at either the start of the page, or it can skip the header
            if (mod != 0 && mod != Data.PageHeaderSize)
            {
                Assert.Fail("Unexpected data offset requested");
            }

            var pageNumber = (offset - Data.HeaderSize - Data.PageManagerHeaderSize) / Data.PageSize;

            if (pageNumber >= this.pageData.Count)
            {
                Assert.Fail("Unexpected page number requested");
            }

            return pageNumber;
        }

        /// <summary>
        /// Creates a binary writer for the given memory stream, resetting the stream to the start.
        /// </summary>
        /// <returns>
        /// A delegate capable of returning the binary writer.
        /// </returns>
        private Func<int, int, BinaryWriter> CreatePageMemoryWriter()
        {
            return (offset, length) =>
            {
                var mod = PageMod(offset);
                var pageNumber = this.GetPageNumberForOffset(offset, length, mod);

                var stream = this.GetPageData(pageNumber);
                stream.Position = mod;
                return new BinaryWriter(stream);
            };
        }

        /// <summary>
        /// Prepares the page data.
        /// </summary>
        private void PreparePageData()
        {
            this.pageData = new List<byte[]>(this.pageSetups.Count);
            foreach (var pageSetup in this.pageSetups)
            {
                // Creating a memory stream that is at most PageSize bytes in length ensures we can't write beyond its extent
                var buffer = new byte[Data.PageSize];
                this.pageData.Add(buffer);
                if (!this.newFile)
                {
                    using (var stream = new MemoryStream(buffer))
                    {
                        pageSetup.WriteTo(stream);
                    }
                }
            }

            this.Mock.Setup(m => m.GetDataReader(It.IsInRange(Data.HeaderSize + Data.PageManagerHeaderSize, int.MaxValue, Range.Inclusive), Data.PageSize - Data.PageHeaderSize)).Returns(this.CreatePageMemoryReader());
            this.Mock.Setup(m => m.GetDataReader(It.IsInRange(Data.HeaderSize + Data.PageManagerHeaderSize, int.MaxValue, Range.Inclusive), Data.PageSize)).Returns(this.CreatePageMemoryReader());
            this.Mock.Setup(m => m.GetDataWriter(It.IsInRange(Data.HeaderSize + Data.PageManagerHeaderSize, int.MaxValue, Range.Inclusive), Data.PageSize)).Returns(this.CreatePageMemoryWriter());
        }

        /// <summary>
        /// Prepares the page header data.
        /// </summary>
        private void PreparePageHeaderData()
        {
            var firstItemDataPage = this.pageSetups.FindIndex(p => !(p is IndexNodePageSetup));
            var firstNodeDataPage = this.pageSetups.FindIndex(p => p is IndexNodePageSetup);
            var firstItemNodeDataPage = this.pageSetups.FindIndex(p => p is ItemNodeIndexPageSetup);

            if (this.newFile)
            {
                this.pageManagerHeaderData = new byte[Data.PageManagerHeaderSize];
            }
            else
            {
                if (firstItemDataPage == -1 || firstNodeDataPage == -1 || firstItemNodeDataPage == -1)
                {
                    throw new Exception("Test setup requires at least one of each page type");
                }

                this.pageManagerHeaderData = Data.Start.Then(firstItemDataPage, firstNodeDataPage, firstItemNodeDataPage, this.pageSetups.Count, 100, 200).ToArray();
            }

            this.Mock.Setup(m => m.GetDataReader(Data.HeaderSize, Data.PageManagerHeaderSize)).Returns(CreateMemoryReader(this.pageManagerHeaderData));
            this.Mock.Setup(m => m.GetDataWriter(Data.HeaderSize, Data.PageManagerHeaderSize)).Returns(CreateMemoryWriter(this.pageManagerHeaderData));
        }

        /// <summary>
        /// Prepares the header data.
        /// </summary>
        private void PrepareHeaderData()
        {
            if (this.newFile)
            {
                this.fileHeaderData = new byte[Data.FileHeaderBytes.Length + 2];
            }
            else
            {
                this.fileHeaderData = Data.FileHeaderBytes.Then(Data.CurrentDataVersion).ToArray();
            }

            this.Mock.Setup(m => m.GetDataReader(0, Data.HeaderSize)).Returns(CreateMemoryReader(this.fileHeaderData));
            this.Mock.Setup(m => m.GetDataWriter(0, Data.HeaderSize)).Returns(CreateMemoryWriter(this.fileHeaderData));
        }
    }
}