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
        bool IsHidden  { get; }

        /// <summary>
        /// Is invoked immediately object is attached to an indexed collection
        /// </summary>
        /// <param name="index">Index of the object within an owning indexed collection</param>
        /// <exception cref="InvalidOperationException">when object is already attached.</exception>
        void Attached(int index, TScope context);

        /// <summary>
        /// Is invoked before object is actually detached from an indexed collection
        /// </summary>
        void Detaching(TScope context);
    }
}
