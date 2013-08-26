using IronText.Framework;

namespace IronText.Lib.Shared
{
    [Vocabulary]
    public class DefFirstNs<TNs> : NsBase<TNs>
    {
        public DefFirstNs(IFrame<TNs> frame = null) : base(frame) { }

        [LanguageService]
        public IParsing Parsing { get; set; }

        [Parse]
        public Ref<TNs> Reference(string var)
        {
            Def<TNs> result = Get(var);
            if (result == null)
            {
                throw new SyntaxException(Parsing.Location, Parsing.HLocation, "Undefined identifier.");
            }

            return result.GetRef();
        }

        [Parse]
        public Def<TNs> Definition(string var)
        {
            if (Frame.Get(var) != null)
            {
                throw new SyntaxException(Parsing.Location, Parsing.HLocation, "Unable to redefine '" + var + "'");
            }
            
            var result = Frame.Define(var);
            return result;
        }

        private Def<TNs> Get(string name)
        {
            Def<TNs> result;

            var node = FrameNode;
            do
            {
                result = node.Value.Get(name);
                if (result != null)
                {
                    break;
                }

                node = node.Next;
            }
            while (node != null);

            return result;
        }
    }
}
