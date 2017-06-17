using IronText.Logging;
using IronText.Runtime;
using Moq;
using NUnit.Framework.Constraints;
using System.IO;

namespace IronText.Testing
{
    internal class ParsableWithTreeByLanguageConstraint<T> : Constraint where T : class
    {
        private string text;
        private Loc location = Loc.Unknown;
        private readonly bool negated;

        public ParsableWithTreeByLanguageConstraint(bool negated)
        {
            this.negated = negated;
        }

        public override bool Matches(object actual)
        {
            this.text = actual as string;
            if (text == null)
            {
                return false;
            }

            return negated
                ? !IsParsable(text)
                : IsParsable(text);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            if (negated)
            {
                writer.Write("text not parsed");
                actual = "text parsed";
            }
            else
            {
                writer.Write("successfully parsed text");
                actual = $"syntax error at {location}.";
            }
        }

        private bool IsParsable(string input)
        {
            var mock = new Mock<T>();
            return IsParsable(mock.Object, input);
        }

        private bool IsParsable(T context, string input)
        {
            using (var interpreter = new Interpreter<T>(context) { LoggingKind = LoggingKind.Collect })
            using (var reader = new StringReader(input))
            {
                var tree = interpreter.BuildTree(reader, Loc.MemoryString);
                return tree != null && interpreter.ErrorCount == 0;
            }
        }
    }
}