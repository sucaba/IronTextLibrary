using System;

namespace IronText.Reflection
{
    /// <summary>
    /// Base production action class
    /// </summary>
    public abstract class ProductionAction : ICloneable
    {
        public ProductionAction Clone()
        {
            return DoClone();
        }

        protected abstract ProductionAction DoClone();

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
