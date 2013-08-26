using System.Text;

namespace IronText.Lib.Sre
{
    public class Chr : Sym
    {
        public Chr(char ch) : base(new string(ch, 1)) { }

        protected override void DoWrite(StringBuilder output)
        {
            output.Append("#\\" + this.Text);
        }       
    }
}
