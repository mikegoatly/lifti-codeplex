namespace Lifti.Persistence
{
    using System;
    using System.Collections.Generic;

    using Lifti.Extensibility;
    using Lifti.Persistence.IO;

    /// <summary>
    /// A page manager implementation that buffers pages until <see cref="IPageManager.Flush"/> is called.
    /// </summary>
    /// <typeparam name="TItem">The type of the item contained in the index.</typeparam>
    public class BufferedPageManager<TItem> : PageManager<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BufferedPageManager&lt;TItem&gt;"/> class.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        /// <param name="settings">The settings for this instance.</param>
        /// <param name="dataFileManager">The data file manager for this instance.</param>
        /// <param name="typePersistence">The type persistence implementation that will manage reading and writing
        /// type data to and from the persistence backing store.</param>
        /// <param name="extensibilityService">The extensibility service.</param>
        public BufferedPageManager(IPageCache pageCache, IPersistenceSettings settings, IDataFileManager dataFileManager, ITypePersistence<TItem> typePersistence, IIndexExtensibilityService<TItem> extensibilityService)
            : base(pageCache, settings, dataFileManager, typePersistence, extensibilityService)
        {
        }

        /// <summary>
        /// The dirty pages.
        /// </summary>
        private readonly Dictionary<IDataPageHeader, IDataPage> dirtyPages = new Dictionary<IDataPageHeader, IDataPage>();

        /// <summary>
        /// Whether or not the page manager header has been modified.
        /// </summary>
        private bool pageManagerHeaderDirty;

        /// <inheritdoc />
        public override void Flush()
        {
            foreach (var page in this.dirtyPages.Values)
            {
                base.PersistPage(page);
            }

            this.dirtyPages.Clear();

            if (this.pageManagerHeaderDirty)
            {
                base.PersistPageManagerHeader();
                this.pageManagerHeaderDirty = false;
            }
        }

        /// <inheritdoc />
        public override IDataPage GetPage(IDataPageHeader pageHeader)
        {
            IDataPage page;
            if (this.dirtyPages.TryGetValue(pageHeader, out page))
            {
                return page;
            }

            return base.GetPage(pageHeader);
        }

        /// <inheritdoc />
        protected override void PersistPageManagerHeader()
        {
            this.pageManagerHeaderDirty = true;
        }

        /// <inheritdoc />
        protected override void PersistPage(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            this.dirtyPages[page.Header] = page;
        }

        /// <inheritdoc />
        protected override void PersistPageHeader(IDataPage page)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            this.dirtyPages[page.Header] = page;
        }
    }
}
