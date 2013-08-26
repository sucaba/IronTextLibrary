using System;

namespace IronText.Lib.Stem
{
    public abstract class Sym : Stx
    {
        public Sym(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            this.Text = text;
        }

        public string Text { get; protected set; }

        #region Object overrides

        public override bool Equals(object obj)
        {
            var syntax = obj as Sym;
            return syntax != null 
                && syntax.GetType() == GetType()
                && syntax.Text == this.Text;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return this.GetType().GetHashCode() + this.Text.GetHashCode();
            }
        }

        #endregion
    }
}
