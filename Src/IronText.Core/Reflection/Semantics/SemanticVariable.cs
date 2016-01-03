using System;

namespace IronText.Reflection
{
    public class SemanticVariable
    {
        /// <summary>
        /// </summary>
        /// <param name="position">
        /// 0 is always reserved for an inherited attribute (outcome or left-side attribute),
        /// positive indexes are used to resolve name duplicates</param>
        /// <param name="name"></param>
        /// negative indexes means first attribute with the name.
        /// <param name="type"></param>
        public SemanticVariable(int position, string name, Type type = null)
        {
            this.Name     = name;
            this.Position = position;
            this.Type     = type ?? typeof(object);
        }

        /// <summary>
        /// Main synthesized result.
        /// </summary>
        /// <param name="type"></param>
        public SemanticVariable(Type type = null)
            : this(0, SynthesizedAttributeNames.Main, type)
        {
        }

        public string Name     { get; private set; }

        public int    Position { get; private set; }

        public Type   Type     { get; private set; }
    }
}