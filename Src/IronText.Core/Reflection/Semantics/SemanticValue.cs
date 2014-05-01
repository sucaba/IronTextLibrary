using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public sealed class SemanticValue
    {
        /// <summary>
        /// No-value constant
        /// </summary>
        public static SemanticValue None { get; private set; }

        static SemanticValue()
        {
            None = new SemanticValue();
        }

        public SemanticValue(string title = null)
        {
            this.Title = title ?? "";
            this.Joint = new Joint();
        }

        public string Title  { get; private set; }

        public Joint  Joint { get; private set; }

        public override string ToString() { return Title; }
    }
}
