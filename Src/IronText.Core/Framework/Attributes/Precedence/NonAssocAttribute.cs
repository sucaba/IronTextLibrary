using System;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=true)]
    public class NonAssocAttribute : BaseAssocAttribute
    {
        public NonAssocAttribute(int value, string term)
            : base(value, term)
        { }

        public NonAssocAttribute(int value, Type term)
            : base(value, term)
        { }

        protected override Associativity GetAssoc() { return Associativity.None; }
    }
}
