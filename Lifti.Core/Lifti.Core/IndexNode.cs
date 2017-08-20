// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A node that represents an indexed character of a word or words within a <see cref="FullTextIndex{TKey}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the index.</typeparam>

    public class IndexNode<TKey>
    {
        private IndexNode<TKey> singleChild;
        private Dictionary<char, IndexNode<TKey>> childNodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexNode{TKey}"/> class.
        /// </summary>
        /// <param name="index">The index the node is a part of.</param>
        /// <param name="indexedCharacter">The indexed character.</param>
        public IndexNode(IFullTextIndex<TKey> index, char indexedCharacter)
        {
            this.IndexedCharacter = indexedCharacter;
            this.Index = index;
        }

        /// <summary>
        /// Gets the parent node in the index.
        /// </summary>
        /// <value>The parent node in the index.</value>
        public IndexNode<TKey> Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the indexed character this node represents.
        /// </summary>
        /// <value>The indexed character.</value>
        public char IndexedCharacter
        {
            get;
        }

        /// <summary>
        /// Gets the index this node is a part of.
        /// </summary>
        /// <value>The associated <see cref="IFullTextIndex{TKey}"/></value>
        public IFullTextIndex<TKey> Index
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this node is a direct match for items.
        /// </summary>
        public bool ContainsDirectItems => this.Items != null && this.Items.Count > 0;

        /// <summary>
        /// Gets or sets the child nodes under this one.
        /// </summary>
        /// <value>The child nodes.</value>

        public virtual IEnumerable<IndexNode<TKey>> ChildNodes
        {
            get
            {
                if (this.singleChild != null)
                {
                    yield return this.singleChild;
                }
                else if (this.childNodes != null)
                {
                    foreach (var child in this.childNodes.Values)
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the items that match a word that is completed at this instance.
        /// </summary>
        /// <value>The list of items.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This needs to be settable by deriving classes to support lazy loading")]
        protected virtual Dictionary<TKey, ItemWordMatch<TKey>> Items
        {
            get;
            set;
        }

        /// <summary>
        /// De-indexes the given item from this node.
        /// </summary>
        /// <param name="item">The item to de-index.</param>
        public virtual void DeindexItem(TKey item)
        {
            if (this.Items.Remove(item))
            {
                this.Index.Extensibility.OnItemWordRemoved(item, this);

                this.CompactIndex();
            }
        }

        /// <summary>
        /// Gets a distinct list of items stored in this node and any of the descendent nodes.
        /// </summary>
        /// <returns>The distinct list of items.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is not just a simple property")]
        public virtual IEnumerable<ItemWordMatch<TKey>> GetDirectAndChildItems()
        {
            return this.GetDirectAndChildItemsUnfiltered().Distinct();
        }

        /// <summary>
        /// Gets a distinct list of items stored in this node and any of the descendent nodes.
        /// </summary>
        /// <returns>The distinct list of items.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is not just a simple property")]
        public virtual IEnumerable<ItemWordMatch<TKey>> GetDirectItems()
        {
            return this.ContainsDirectItems ? this.Items.Values : Enumerable.Empty<ItemWordMatch<TKey>>();
        }

        /// <summary>
        /// Indexes the the given item against the given word.
        /// </summary>
        /// <param name="item">The item being indexed.</param>
        /// <param name="word">The word the item is being indexed against.</param>
        /// <param name="locations">The locations the word was indexed at for the item's text.</param>
        /// <returns>The node at the end of the indexed word.</returns>
        public virtual IndexNode<TKey> IndexItem(TKey item, string word, int[] locations)
        {
            return this.IndexItemCharacter(new ItemWordMatch<TKey>(item, locations), word, 0);
        }

        /// <summary>
        /// Matches the specified letter returning the child node that represents the character.
        /// </summary>
        /// <param name="letter">The letter to match.</param>
        /// <returns>The matching child node, or null if the given letter does not match any child node.</returns>
        public virtual IndexNode<TKey> Match(char letter)
        {
            IndexNode<TKey> childNode = null;

            if (this.singleChild != null)
            {
                if (letter == this.singleChild.IndexedCharacter)
                {
                    childNode = this.singleChild;
                }
            }
            else if (this.childNodes != null)
            {
                this.childNodes.TryGetValue(letter, out childNode);
            }

            return childNode;
        }

        ///// <summary>
        ///// Gets the child nodes associated to this item.
        ///// </summary>
        ///// <returns>The child nodes contained under this node.</returns>
        //internal IEnumerable<IndexNode<TKey>> GetChildNodes()
        //{
        //    return this.ChildNodes == null ? Enumerable.Empty<IndexNode<TKey>>() : this.ChildNodes.Values;
        //}

        /// <summary>
        /// Removes reference to the given character from this index node. Does not throw an exception if it doesn't exist.
        /// </summary>
        /// <param name="character">The character to remove.</param>
        public virtual void Remove(char character)
        {
            if (this.singleChild != null)
            {
                if (this.singleChild.IndexedCharacter == character)
                {
                    this.singleChild = null;
                }
            }
            else
            {
                this.childNodes?.Remove(character);
            }
        }

        /// <summary>
        /// Clears this instance, removing any direcly indexed items and recursively clearing any of its children.
        /// </summary>
        public virtual void Clear()
        {
            if (this.singleChild != null)
            {
                this.singleChild.Clear();
                this.singleChild = null;
            }
            else if (this.childNodes != null)
            {
                foreach (var node in this.ChildNodes)
                {
                    node.Clear();
                }

                this.childNodes.Clear();
                this.childNodes = null;
            }

            if (this.Items != null)
            {
                this.Items.Clear();
                this.Items = null;
            }
        }

        /// <summary>
        /// Gets all the child items, both directly in this node and in the child nodes. Duplicate items
        /// may be returned from this method.
        /// </summary>
        /// <returns>The items stored both in this instance, and in all the child instances.</returns>
        private IEnumerable<ItemWordMatch<TKey>> GetDirectAndChildItemsUnfiltered()
        {
            if (this.ContainsDirectItems)
            {
                foreach (var item in this.Items.Values)
                {
                    yield return item;
                }
            }

            // TODO unwrap this linq - use hashset
            var childItems = from c in this.ChildNodes
                             from i in c.GetDirectAndChildItemsUnfiltered()
                             select i;

            foreach (var item in childItems.Distinct())
            {
                yield return item;
            }
        }

        /// <summary>
        /// Indexes the the given item against the specified character in the given word.
        /// </summary>
        /// <param name="item">The item being indexed.</param>
        /// <param name="word">The word the item is being indexed against.</param>
        /// <param name="characterIndex">The current index of the character in the word that is currently being processed.</param>
        /// <returns>The node at the end of the indexed word.</returns>
        private IndexNode<TKey> IndexItemCharacter(ItemWordMatch<TKey> item, string word, int characterIndex)
        {
            if (characterIndex == word.Length)
            {
                // This node represents the last character of the word
                this.AddNodeItem(item);
                return this;
            }

            var currentCharacter = word[characterIndex];
            var childNode = this.GetOrCreateChildNode(currentCharacter);

            // Index the next character of the word in the child node
            return childNode.IndexItemCharacter(item, word, ++characterIndex);
        }

        /// <summary>
        /// Gets the child node for the given character, or creates it if there isn't already one.
        /// </summary>
        /// <param name="character">The character to get or create the child node for.</param>
        /// <returns>The child node.</returns>
        private IndexNode<TKey> GetOrCreateChildNode(char character)
        {
            var childNode = this.Match(character);

            // Check if there is already a child node for the next character
            if (childNode == null)
            {
                // Create a new child node for the next character of the word
                childNode = this.Index.CreateIndexNode(character);
                childNode.Parent = this;

                this.Index.Extensibility.OnNodeCreated(childNode);

                this.AddNewChildNode(childNode);
            }

            return childNode;
        }

        /// <summary>
        /// Adds the item to this instances items collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        private void AddNodeItem(ItemWordMatch<TKey> item)
        {
            if (this.Items == null)
            {
                this.Items = new Dictionary<TKey, ItemWordMatch<TKey>>();
            }

            // Add the item to this node's collection
            this.Items.Add(item.Item, item);

            this.Index.Extensibility.OnItemWordIndexed(item, this);
        }

        /// <summary>
        /// Compacts this indexed node, removing it from the index if it has no items contained at it
        /// or under it.
        /// </summary>
        private void CompactIndex()
        {
            if (this.Parent == null)
            {
                // Never compact the root node.
                return;
            }

            if (!this.ContainsDirectItems && this.singleChild == null && (this.childNodes == null || this.childNodes.Count == 0))
            {
                this.Index.Extensibility.OnNodeRemoved(this);

                this.Parent.Remove(this.IndexedCharacter);
                this.Parent.CompactIndex();

                // Null out the references to help the GC
                this.Items = null;
                this.childNodes = null;
                this.Parent = null;
            }
        }

        /// <summary>
        /// Adds the given child node to this instance.
        /// </summary>
        /// <param name="childNode">
        /// The node to add.
        /// </param>
        protected void AddNewChildNode(IndexNode<TKey> childNode)
        {
            if (this.singleChild == null && this.childNodes == null)
            {
                this.singleChild = childNode;
            }
            else if (this.singleChild != null)
            {
                this.childNodes = new Dictionary<char, IndexNode<TKey>>
                                  {
                                      { this.singleChild.IndexedCharacter, this.singleChild },
                                      { childNode.IndexedCharacter, childNode }
                                  };

                this.singleChild = null;
            }
            else
            {
                this.childNodes.Add(childNode.IndexedCharacter, childNode);
            }
        }
    }
}
