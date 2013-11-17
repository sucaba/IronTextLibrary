using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework.Reflection
{
    public interface ITableObject
    {
        /// <summary>
        /// Unique object identifier
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Determinise if object is detached from a table
        /// </summary>
        bool IsDetached { get; }

        /// <summary>
        /// Attaches object to a table.
        /// </summary>
        /// <param name="id">Unique ID of the object within an owning table</param>
        /// <exception cref="InvalidOperationException">when object is already attached.</exception>
        void Attach(int id);

        /// <summary>
        /// Detaches object from a table
        /// </summary>
        void Detach();
    }
}
