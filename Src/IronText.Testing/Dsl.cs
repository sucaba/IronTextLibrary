using System;
using NUnit.Framework.Constraints;

namespace IronText.Testing
{
    public static class Dsl
    {
        public static Constraint ParsableBy<T>()
            where T : class
            => new IsSyntax().ParsableBy<T>();

        public static Constraint ParsableWithTreeBy<T>()
            where T : class
            => new IsSyntax().ParsableWithTreeBy<T>();

        public static IsSyntax Not => new IsSyntax(negated: true);

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

            public Constraint ParsableWithTreeBy<T>()
                where T : class
            {
                return new ParsableWithTreeByLanguageConstraint<T>(negated);
            }
        }
    }
}
