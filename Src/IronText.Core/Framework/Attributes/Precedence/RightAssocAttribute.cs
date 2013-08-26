using System;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=true)]
    public class RightAssocAttribute : BaseAssocAttribute
    {
        public RightAssocAttribute(int value, string term)
            : base(value, term)
        { }

        public RightAssocAttribute(int value, Type term)
            : base(value, term)
        { }

        protected override Associativity GetAssoc() { return Associativity.Right; }
    }
}
