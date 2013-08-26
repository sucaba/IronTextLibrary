using System;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface, AllowMultiple=true)]
    public class LeftAssocAttribute : BaseAssocAttribute
    {
        public LeftAssocAttribute(int value, string term)
            : base(value, term)
        { }

        public LeftAssocAttribute(int value, Type term)
            : base(value, term)
        { }

        protected override Associativity GetAssoc() { return Associativity.Left; }
    }
}
