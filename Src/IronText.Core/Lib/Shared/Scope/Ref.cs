
namespace IronText.Lib.Shared
{
    public class Ref<TNs>
    {
        public Ref(Def<TNs> def) { this.Def = def; }

        public readonly Def<TNs> Def;
        public object Value { get { return Def.Value; } }
        public int Tag;
    }
}
