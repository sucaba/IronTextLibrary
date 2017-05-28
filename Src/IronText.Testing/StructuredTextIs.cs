using NUnit.Framework.Constraints;

namespace IronText.Testing
{
    public static class StructuredText
    {
        public static IsSyntax Is => new IsSyntax();

        public class IsSyntax
        {
            private readonly bool negated;

            public IsSyntax(bool negated = false)
            {
                this.negated = negated;
            }

            public IsSyntax Not => new IsSyntax(!negated);

            public Constraint ParsableBy<T>()
                where T : class
            {
                return new ParsableByLanguageConstraint<T>(negated);
            }
        }
    }
}
