using IronText.Framework;
using System.Collections.Generic;

namespace IronText.Tests.Framework.Generic
{
    public static class GrammarsUnderTest
    {
        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("LeftRecursion0.gv")]
        [DescribeParserStateMachine("LeftRecursion0.info")]
        public interface LeftRecursion
        {
            [Produce]
            void All(A s);

            [Produce]
            A S();

            [Produce(null, "a")]
            A S(A before);
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph("HiddenLeftRecursion0.gv")]
        [DescribeParserStateMachine("HiddenLeftRecursion0.info")]
        public interface HiddenLeftRecursion
        {
            [Produce]
            void All(S s);

            [Produce]
            S S();

            [Produce(null, null, "a")]
            S S(S s1, S s2);
        }

        [Language(RuntimeOptions.ForceGeneric)]
        [ParserGraph(nameof(TrivialMergeLanguage) + "0.gv")]
        [DescribeParserStateMachine(nameof(TrivialMergeLanguage) + "0.info")]
        public class TrivialMergeLanguage
        {
            public List<string> Result { get; } = new List<string>();

            [Produce]
            public void SetResult(string text)
            {
                Result.Add(text);
            }

            [Produce]
            public string Concat(bool first, bool second) => first + "," + second;

            [Merge]
            public string Merge(string x, string y) => x + "|" + y;

            [Merge]
            public bool Merge(bool x, bool y) => y;

            [Produce]
            public bool X() => false;

            [Produce("a")]
            public bool Xa() => true;
        }


        public interface A {}
        public interface S {}
    }
}
