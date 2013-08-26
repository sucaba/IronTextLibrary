using System.Text;

namespace IronText.Lib.Stem
{
    public class Num : Sym
    {
        public Num(string text) : base(text) { }

        protected override void DoWrite(StringBuilder output)
        {
            output.Append(this.Text);
        }       
    }
}
