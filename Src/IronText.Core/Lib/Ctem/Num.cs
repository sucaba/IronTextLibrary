
namespace IronText.Lib.Ctem
{
    public sealed class Num
    {
        public Num(string text) { this.Text = text; }

        public string Text { get; private set; }

        public override bool Equals(object obj)
        {
            var casted = obj as Num;
            return casted != null && casted.Text == Text;
        }

        public override int GetHashCode()
        {
            return Text == null ? 0 : Text.GetHashCode();
        }
    }
}
