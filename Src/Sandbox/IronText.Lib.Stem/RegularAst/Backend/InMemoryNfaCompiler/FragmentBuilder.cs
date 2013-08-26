using System.Collections.Generic;
using System.Linq;
using IronText.Lib.RegularAst.Backend.InMemoryNfaCompiler;

namespace IronText.Lib.RegularAst
{
    class FragmentBuilder : IAstNodeVisitor<NfaFragment>
    {
        public NfaFragment Visit(CharSetNode node)
        {
            var s = new NfaState(node.Characters);
            return new NfaFragment
            {
                Start = s,
                Outs = new List<List<NfaState>> { s.Out }
            };
        }

        public NfaFragment Visit(ActionNode node)
        {
            return NfaFragment.Cat(new NfaFragment[0]);
        }

        public NfaFragment Visit(CatNode node)
        {
            return NfaFragment.Cat(node.Children.Select(Build));
        }

        public NfaFragment Visit(OrNode node)
        {
            return NfaFragment.Or(node.Children.Select(Build));
        }

        public NfaFragment Visit(RepeatNode node)
        {
            // TODO: Replace Range with repeat
            IEnumerable<NfaFragment> required 
                = Enumerable
                    .Range(0, node.MinCount)
                    .Select(_ => Build(node.Inner));

            IEnumerable<NfaFragment> optional;
            if (node.MaxCount != int.MaxValue)
            {
                optional = Enumerable
                               .Range(0, node.MaxCount - node.MinCount)
                               .Select(_ => NfaFragment.ZeroOrOne(Build(node.Inner)));
            }
            else
            {
                optional = new [] { NfaFragment.ZeroOrMore(Build(node.Inner)) };
            }

            return NfaFragment.Cat(required.Concat(optional));
        }

        public NfaFragment Build(AstNode program)
        {
            return program.Accept(this);
        }
    }
}
