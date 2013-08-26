using System.Collections.Generic;
using System.Text;
using IronText.Automata.Regular;
using IronText.Lib.RegularAst;
using IronText.Tests.Algorithm;
using NUnit.Framework;

namespace IronText.Tests.Lib.Sre
{
    [TestFixture]
    public class when_compiling_regular_expression_to_DFA
    {
        private RegularTree regularTree;
        private IDfaSerialization serialization;
        private IDfaSimulation simulation;

        [Test]
        public void DFA_matches_input_correctly()
        {
            // Given alphabet
            const int A = 0;
            const int B = 1;

            GivenRegularExpression(
                new CatNode(new List<AstNode> {
                        RepeatNode.ZeroOrMore(
                            new OrNode(new List<AstNode> { 
                                CharSetNode.Create(A),
                                CharSetNode.Create(B)
                            })),
                        CharSetNode.Create(A),
                        CharSetNode.Create(B),
                        CharSetNode.Create(B)
                    })
                );

            CreateDfa();

            DfaShouldMatch(A, B, B);
            DfaShouldMatch(A, A, A, A, B, B);
            DfaShouldMatch(B, B, B, A, B, B);
            DfaShouldMatch(A, B, B, A, B, B);

            DfaShouldNotMatch();
            DfaShouldNotMatch(A, B);
            DfaShouldNotMatch(A, B, B, A);
            DfaShouldNotMatch(A, B, B, B);

            DfaShouldMatchBeginning(A, B, B);
            DfaShouldMatchBeginning(A, A, A, A, B, B);
            DfaShouldMatchBeginning(B, B, B, A, B, B);
            DfaShouldMatchBeginning(A, B, B, A, B, B);
            DfaShouldMatchBeginning(A, B, B, A);
            DfaShouldMatchBeginning(A, B, B, B);

            DfaShouldNotMatchBeginning();
            DfaShouldNotMatchBeginning(A, B, A, B);
            DfaShouldNotMatchBeginning(A, A, A, A);
            DfaShouldNotMatchBeginning(B, B, B, B);
        }

        [Test]
        public void DFA_triggers_actions_correctly()
        {
            // Given alphabet
            const int A = 0;
            const int B = 1;

            // Given actions
            const int Action0 = 1001;
            const int Action1 = 1002;
            const int Action2 = 1003;

            GivenRegularExpression(
                AstNode.Or(
                    AstNode.Cat(
                        CharSetNode.Create(A),          // 0
                        ActionNode.Create(Action0)),    // 1
                    AstNode.Cat(                        // 
                        CharSetNode.Create(A),          // 2
                        CharSetNode.Create(B),          // 3
                        CharSetNode.Create(B),          // 4
                        ActionNode.Create(Action1)      // 5
                        ),
                    AstNode.Cat(
                        RepeatNode.ZeroOrMore(CharSetNode.Create(A)), // 6
                        RepeatNode.OneOrMore(CharSetNode.Create(B)),  // 7, 8
                        ActionNode.Create(Action2))                   // 9
                ));

            CreateDfa();

            DfaShouldTriggerAction(Action0, A);
            DfaShouldTriggerAction(Action1, A, B, B);
            DfaShouldTriggerAction(Action2, A, B);
            DfaShouldTriggerAction(Action2, A, B, B, B);
        }

        [Test]
        public void input_equivalence_classes_are_used()
        {
            // Given alphabet
            int A = 0;
            int B = 0x10FFFF; // Unicode max

            GivenRegularExpression(
                new CatNode(new List<AstNode> {
                        RepeatNode.ZeroOrMore( CharSetNode.CreateRange(A, B) ),
                        CharSetNode.Create(A),
                        CharSetNode.Create(B),
                        CharSetNode.Create(B)
                    })
                );

            CreateDfa();

            DfaShouldMatch(A, B, B);
            DfaShouldMatch(A, A, A, A, B, B);
            DfaShouldMatch(B, B, B, A, B, B);
            DfaShouldMatch(A, B, B, A, B, B);

            DfaShouldNotMatch();
            DfaShouldNotMatch(A, B);
            DfaShouldNotMatch(A, B, B, A);
            DfaShouldNotMatch(A, B, B, B);

            DfaShouldMatchBeginning(A, B, B);
            DfaShouldMatchBeginning(A, A, A, A, B, B);
            DfaShouldMatchBeginning(B, B, B, A, B, B);
            DfaShouldMatchBeginning(A, B, B, A, B, B);
            DfaShouldMatchBeginning(A, B, B, A);
            DfaShouldMatchBeginning(A, B, B, B);

            DfaShouldNotMatchBeginning();
            DfaShouldNotMatchBeginning(A, B, A, B);
            DfaShouldNotMatchBeginning(A, A, A, A);
            DfaShouldNotMatchBeginning(B, B, B, B);
        }

        [Test]
        public void dfa_provides_correct_serialization_information()
        {
            // Given alphabet
            const int A = 0;
            const int B = 1;

            // Given actions
            const int Action0 = 1001;
            const int Action1 = 1002;
            const int Action2 = 1003;

            GivenRegularExpression(
                AstNode.Or(
                    AstNode.Cat(
                        CharSetNode.Create(A),
                        ActionNode.Create(Action0)),
                    AstNode.Cat(
                        CharSetNode.Create(A),
                        CharSetNode.Create(B),
                        CharSetNode.Create(B),
                        ActionNode.Create(Action1)
                        ),
                    AstNode.Cat(
                        RepeatNode.ZeroOrMore(CharSetNode.Create(A)),
                        RepeatNode.OneOrMore(CharSetNode.Create(B)),
                        ActionNode.Create(Action2))
                ));

            CreateDfa();
            var output = new StringBuilder();
            serialization.Print(output);
            // DUBUG: Console.WriteLine(output);
        }

        private void DfaShouldTriggerAction(int expectedAction, params int[] input)
        {
            int action;
            bool success = simulation.Scan(input, out action);
            Assert.IsTrue(success);
            Assert.AreEqual(expectedAction, action);
        }

        private void DfaShouldMatchBeginning(params int[] input)
        {
            Assert.IsTrue(simulation.MatchBeginning(input));
        }

        private void DfaShouldNotMatchBeginning(params int[] input)
        {
            Assert.IsFalse(simulation.MatchBeginning(input));
        }

        private void DfaShouldMatch(params int[] input)
        {
            Assert.IsTrue(simulation.Match(input));
        }

        private void DfaShouldNotMatch(params int[] input)
        {
            Assert.IsFalse(simulation.Match(input));
        }

        private void CreateDfa()
        {
            var dfa = new RegularToDfaAlgorithm(regularTree);
            this.simulation = new DfaSimulation(dfa.Data);
            this.serialization = new DfaSerialization(dfa.Data);

            // DEBUG: Console.WriteLine(dfa);
        }

        private void GivenRegularExpression(AstNode astNode)
        {
            this.regularTree = new RegularTree(astNode);
        }
    }
}
