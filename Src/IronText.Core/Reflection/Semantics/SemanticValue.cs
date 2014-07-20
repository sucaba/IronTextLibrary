using System;
using IronText.Collections;

namespace IronText.Reflection
{
    public sealed class SemanticValue
    {
        [NonSerialized]
        private readonly Joint _joint = new Joint();

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
        }

        public string Title  { get; private set; }

        public Joint  Joint { get { return _joint; } }

        public override string ToString() { return Title; }
    }
}
