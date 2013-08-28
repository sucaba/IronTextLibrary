using System.Text;
using IronText.Framework;

namespace IronText.Lib.Stem
{   
    /// <summary>
    /// Base syntax object class.
    /// </summary>
    /// <remarks>
    /// Contains optional source syntax information for inherited datums.
    /// </remarks>
    public abstract class Stx
    {
        public Loc Location { get; set; }

        #region Object overrides 

        public override string ToString()
        {
            var output = new StringBuilder();
            this.Write(output);
            return output.ToString();
        }

        #endregion

        public void Write(StringBuilder output)
        {
            this.DoWrite(output);
        }

        protected abstract void DoWrite(StringBuilder output);
    }
}
