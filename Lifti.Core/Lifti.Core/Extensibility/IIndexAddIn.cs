// This source is subject to the the MIT License (MIT)
// All rights reserved.

namespace Lifti.Extensibility
{
    /// <summary>
    /// The interface implemented by add-ins that can be plugged into a <see cref="FullTextIndex&lt;TKey&gt;"/> instance.
    /// </summary>
    /// <typeparam name="TKey">The type of the key stored in the full text index this add-in will be associated with.</typeparam>
    public interface IIndexAddIn<TKey>
    {
        /// <summary>
        /// Initializes this instance, allowing the add-in code to hook into the required
        /// extensibility points.
        /// </summary>
        /// <param name="extensibilityService">The extensibility service.</param>
        void Initialize(IIndexExtensibilityService<TKey> extensibilityService);
    }
}
