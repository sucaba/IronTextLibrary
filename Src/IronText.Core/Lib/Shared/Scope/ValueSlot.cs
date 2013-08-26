using System.Collections.Generic;

namespace IronText.Lib.Shared
{
    public class ValueSlot<TNs> : Def<TNs>
    {
        private readonly List<Ref<TNs>> refsBeforeDef = new List<Ref<TNs>>();

        public string Name { get; set; }

        public Ref<TNs> GetRef()
        {
            var result = new Ref<TNs>(this);
            if (!IsExplicit)
            {
                refsBeforeDef.Add(result);
            }

            return result;
        }

        public object Value { get; set; }

        public bool IsExplicit { get; set; }

        public IEnumerable<Ref<TNs>> RefsBefore { get { return refsBeforeDef; } }
    }
}
