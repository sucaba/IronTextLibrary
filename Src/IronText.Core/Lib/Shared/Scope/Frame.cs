using System.Collections.Generic;

namespace IronText.Lib.Shared
{
    public class Frame<TNs> : IFrame<TNs>
    {
        private readonly Dictionary<string,Def<TNs>> Items = new Dictionary<string,Def<TNs>>();

        public Def<TNs> Get(string name)
        {
            Def<TNs> result;
            Items.TryGetValue(name, out result);
            return result;
        }

        public virtual Def<TNs> Define() { return new ValueSlot<TNs>(); }

        public virtual Def<TNs> Define(string name)
        {
            var result = Define();
            result.Name = name;
            Items.Add(name, result);
            return result;
        }
    }
}
