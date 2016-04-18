using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Collections
{
    internal interface IIndexableImpl<TScope>
    {
        void MarkSoftRemoved();

        /// <summary>
        /// Is invoked immediately object is attached to an indexed collection
        /// </summary>
        /// <param name="index">Index of the object within an owning indexed collection</param>
        /// <exception cref="InvalidOperationException">when object is already attached.</exception>
        void Attached(TScope scope);

        /// <summary>
        /// Is invoked before object is actually detached from an indexed collection
        /// </summary>
        void Detaching(TScope scope);

        void AssignIndex(int index);
    }
}
