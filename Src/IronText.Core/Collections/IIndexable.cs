using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Collections
{
    public interface IIndexable
    {
        /// <summary>
        /// Determinise if object is detached from an indexed collection
        /// </summary>
        bool IsDetached { get; }

        /// <summary>
        /// Attaches object to an indexed collection
        /// </summary>
        /// <param name="index">Index of the object within an owning indexed collection</param>
        /// <exception cref="InvalidOperationException">when object is already attached.</exception>
        void Attach(int index);

        /// <summary>
        /// Detaches object from an indexed collection
        /// </summary>
        void Detach();
    }
}
