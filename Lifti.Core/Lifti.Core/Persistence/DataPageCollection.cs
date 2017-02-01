// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Persistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A managed collection of data pages numbers
    /// </summary>
    public class DataPageCollection : IDataPageCollection
    {
        /// <summary>
        /// The inner list.
        /// </summary>
        private readonly List<IDataPageHeader> innerList = new List<IDataPageHeader>();

        /// <summary>
        /// The last header that was yielded from a search. This is used to optimise repeated searches for the 
        /// same id, or ids neighbouring the previous search value.
        /// </summary>
        private int? lastSearchResultIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPageCollection"/> class.
        /// </summary>
        public DataPageCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataPageCollection"/> class.
        /// </summary>
        /// <param name="headers">The headers to initially populate the collection with.</param>
        public DataPageCollection(IEnumerable<IDataPageHeader> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            this.innerList.AddRange(headers);
        }

        /// <summary>
        /// Gets the number of pages managed by the collection.
        /// </summary>
        /// <value>The number of pages.</value>
        public int Count
        {
            get { return this.innerList.Count; }
        }

        /// <summary>
        /// Inserts the given page as the first page in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        public void InsertFirst(IDataPageHeader newPage)
        {
            this.innerList.Insert(0, newPage);
            this.lastSearchResultIndex = null;
        }

        /// <summary>
        /// Inserts the given page as the last page in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        public void InsertLast(IDataPageHeader newPage)
        {
            this.innerList.Add(newPage);
            this.lastSearchResultIndex = null;
        }

        /// <summary>
        /// Inserts the specified given page immediately after another in the list.
        /// </summary>
        /// <param name="newPage">The new page.</param>
        /// <param name="afterPage">The page to insert after.</param>
        public void Insert(IDataPageHeader newPage, IDataPageHeader afterPage)
        {
            var index = this.innerList.IndexOf(afterPage);
            if (index == -1)
            {
                throw new ArgumentException("Page not in list", nameof(afterPage));
            }

            this.innerList.Insert(index + 1, newPage);

            this.lastSearchResultIndex = null;
        }

        /// <summary>
        /// Removes the specified page.
        /// </summary>
        /// <param name="page">The page to remove.</param>
        public void Remove(IDataPageHeader page)
        {
            var index = this.innerList.IndexOf(page);
            if (index == -1)
            {
                throw new ArgumentException("Page not in list", nameof(page));
            }

            this.innerList.RemoveAt(index);

            this.lastSearchResultIndex = null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator&lt;T&gt;"/> object that can be used to iterate through this instance.
        /// </returns>
        public IEnumerator<IDataPageHeader> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        /// <summary>
        /// Finds the pages that contain the entries for the given entry.
        /// </summary>
        /// <param name="id">The internal id of the entry to locate the pages for.</param>
        /// <returns>The data page headers for the pages that contain the entry.</returns>
        public IEnumerable<IDataPageHeader> FindPagesForEntry(int id)
        {
            if (this.Count == 0)
            {
                yield break;
            }

            // Binary search through the list
            var index = this.SeekIndexForId(id);

            // Work back from the final mid point until the first entry for the id is found
            while (index > 0 && this.innerList[index - 1].LastEntry == id)
            {
                index--;
            }

            // Yield the relevant pages, starting from the first identified page
            do
            {
                var current = this.innerList[index];
                if (current.FirstEntry <= id && current.LastEntry >= id)
                {
                    yield return current;
                }
                else
                {
                    yield break;
                }

                index++;
            }
            while (index < this.innerList.Count);
        }

        /// <summary>
        /// Finds the page that is the closest match to containing the given id.
        /// </summary>
        /// <param name="id">The internal id of the entry to locate the closest page for.</param>
        /// <returns>The data page header for the page that is closest to containing the given id.</returns>
        public IDataPageHeader FindClosestPageForEntry(int id)
        {
            if (this.Count == 0)
            {
                return null;
            }

            // Binary search through the list
            var index = this.SeekIndexForId(id);

            var current = this.innerList[index];
            if (current.FirstEntry >= id)
            {
                // Work backwards until the first page that starts before the id
                while (index > 0 && 
                    (current.FirstEntry > id || 
                    (current.FirstEntry == id && this.innerList[index - 1].LastEntry == id)))
                {
                    current = this.innerList[--index];
                }
            }
            else if (current.LastEntry < id && index < this.Count - 1)
            {
                do
                {
                    var next = this.innerList[++index];
                    if (next.FirstEntry > id)
                    {
                        break;
                    }

                    current = next;
                }
                while (index < this.Count - 1 && current.LastEntry < id);
            }

            return current;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through this instance.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Seeks the index of the header in this collection that contains the given id, or is closest to matching the id. 
        /// Note that this will only return *a* page that contains, not the first instance of the page if multiple pages contain the given id.
        /// </summary>
        /// <param name="id">The id to find.</param>
        /// <returns>The index of a page in the collection that contains the id.</returns>
        private int SeekIndexForId(int id)
        {
            // First check to see if the last returned index is appropriate for the id
            IDataPageHeader current;
            if (this.lastSearchResultIndex != null)
            {
                current = this.innerList[this.lastSearchResultIndex.GetValueOrDefault()];
                if (current.FirstEntry <= id && current.LastEntry >= id)
                {
                    return this.lastSearchResultIndex.GetValueOrDefault();
                }
            }

            // Perform a binary search for a page containing the id
            var left = 0;
            var right = this.innerList.Count - 1;
            int mid;

            do
            {
                mid = left + ((right - left) >> 1);
                current = this.innerList[mid];
                if (id > current.LastEntry)
                {
                    left = mid + 1;
                }
                else if (id < current.FirstEntry)
                {
                    right = mid - 1;
                }
                else
                {
                    break;
                }
            }
            while (left <= right);

            this.lastSearchResultIndex = mid;
            return mid;
        }
    }
}
