using IronText.Framework;
using System.Collections.Generic;

namespace IronText.Lib.Shared
{
    [Vocabulary]
    public class OnDemandNs<TNs> : NsBase<TNs>
    {
        public OnDemandNs(IFrame<TNs> frame = null) : base(frame) { }

        public IEnumerable<Ref<TNs>> RefsBefore(Def<TNs> def) { return def.RefsBefore; }

        [Produce]
        public Ref<TNs> Reference(string var)
        {
            var def = Get(var) ?? Frame.Define(var);
            return def.GetRef();
        }

        [Produce(HasSideEffect = true)]
        public Def<TNs> Definition(string var)
        {
            // find also implicit definitions in this frame
            var result = Frame.Get(var) ?? Frame.Define(var);
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
