using IronText.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    /// <summary>
    /// Action parameter which value will be injected from the nearest source.
    /// </summary>
    /// <remarks>
    /// The most relevant concept to this class is *inherited attribute*. However inherited
    /// attribute is applied to symbols and parser states while 'injected parameter' is more
    /// friendly in a sense that it is close to the actual user requirement.
    /// For instance, if user wants to access current local scope of some language in some action,
    /// he just needs to add injected parameter of type 'Environment' which will be resolved 
    /// in runtime to the nearest defined scope.
    /// </remarks>
    public class InjectedActionParameter : IndexableObject<IGrammarScope>
    {
        /// <summary>
        /// Creates instance of the <see cref="InjectedActionParameter"/>
        /// </summary>
        /// <param name="production"></param>
        /// <param name="position"></param>
        /// <exception cref="ArgumentNullException">when <see cref="production"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentAutOfRangeException">when position is less then 0 greather then production top-level component count.</exception>
        public InjectedActionParameter(Production production, int position)
        {
            if (production == null)
            {
                throw new ArgumentNullException("production");
            }

            if (position < 0 || position > production.Components.Length)
            {
                throw new ArgumentOutOfRangeException("position");
            }

            this.Production = production;
            this.Position   = position;
        }

        public Production Production { get; private set; }

        public int        Position   { get; private set; }
    }
}
