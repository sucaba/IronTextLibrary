using System;

namespace IronText.Collections
{
    public interface IIndexable<TScope>
    {
        /// <summary>
        /// Determines if object is detached from an indexed collection
        /// </summary>
        bool IsDetached { get; }

        /// <summary>
        /// Determines if object is hidden i.e. it is present in collection but
        /// is not enumerated or accessable by index.
        /// </summary>
        bool IsSoftRemoved { get; }
    }
}
