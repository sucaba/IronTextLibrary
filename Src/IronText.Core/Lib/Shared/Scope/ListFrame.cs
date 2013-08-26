using System.Collections.Generic;

namespace IronText.Lib.Shared
{
    public class ListFrame<TNs,T> : Frame<TNs> where T : class
    {
        public IList<T> Items;

        public override Def<TNs> Define() { return new ListEntryCell<TNs,T>(Items); }
    }

    class ListEntryCell<TNs,T> : Def<TNs> where T : class
    {
        private readonly IList<T> list;
        private T entry;
        private readonly List<Ref<TNs>> refsBeforeDef = new List<Ref<TNs>>();

        public ListEntryCell(IList<T> list) { this.list = list; }

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

        public object Value
        {
            get { return entry; }
            set
            {
                if (entry != null && entry != value)
                {
                    list.Remove(entry);
                }

                entry = (T)value;
                list.Add(entry);
            }
        }

        public bool IsExplicit { get; set; }

        public IEnumerable<Ref<TNs>> RefsBefore { get { return refsBeforeDef; } }
    }
}
