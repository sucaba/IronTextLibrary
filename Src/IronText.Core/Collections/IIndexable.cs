﻿using System;

namespace IronText.Collections
{
    public interface IIndexable<TScope>
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
        void Attach(int index, TScope context);

        /// <summary>
        /// Detaches object from an indexed collection
        /// </summary>
        void Detach(TScope context);
    }
}
