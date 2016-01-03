namespace IronText.Reflection
{
    public class SemanticReference
    {
        /// <summary>
        /// </summary>
        /// <param name="position">
        /// 0 is always reserved for an inherited attribute (outcome or left-side attribute),
        /// positive indexes are used to resolve name duplicates</param>
        /// <param name="name"></param>
        /// negative indexes means first attribute with the name.
        public SemanticReference(int position, string name)
        {
            this.Name     = name;
            this.Position = position;
        }

        /// <summary>
        /// Reference to the right-side main synthesized attribute.
        /// </summary>
        /// <param name="position"></param>
        public SemanticReference(int position)
            : this(position, SynthesizedAttributeNames.Main)
        {
        }

        public string Name     { get; private set; }

        public int    Position { get; private set; }
    }
}