using System;
using System.Linq;
using IronText.Lib.NfaVM;
using IronText.Lib.Shared;

namespace IronText.Lib.RegularAst
{
    public class RegularNfaVMCompiler : IAstNodeVisitor<INfaVM,INfaVM>
    {
        public INfaVM Compile(AstNode root, INfaVM code)
        {
            code.Save(0);
            root.Accept(this, code);
            code.Save(1);
            code.Match();
            return code;
        }

        public INfaVM Visit(CharSetNode node, INfaVM code)
        {
            code.Fetch();
            if (node.Characters.Count != 1)
            {
                throw new NotImplementedException("TODO: ISreVM Compilation of multiple charact csets.");
            }

            code.IsA(node.Characters.First());
            return code;
        }

        public INfaVM Visit(ActionNode node, INfaVM code)
        {
            throw new NotImplementedException("Action compilation is not yet supported by INfaVM compiler");
        }

        public INfaVM Visit(CatNode node, INfaVM code)
        {
            foreach (var child in node.Children)
            {
                child.Accept(this, code);
            }

            return code;
        }

        public INfaVM Visit(OrNode node, INfaVM code)
        {
            int count = node.Children.Count;
            Ref<Labels>[] branch = new Ref<Labels>[count - 1];
            Ref<Labels> end = code.Labels.Generate().GetRef();

            for (int i = 1; i < count; ++i)
            {
                branch[i - 1] = code.Labels.Generate().GetRef();
                code.Fork(branch[i - 1]);
            }

            for (int i = 0; i != count; ++i)
            {
                if (i != 0)
                {
                    code.Label(branch[i - 1].Def);
                }

                node.Children[i].Accept(this, code);
                if (i != (count - 1))
                {
                    code.Jmp(end);
                }
            }

            code.Label(end.Def);

            return code;
        }

        public INfaVM Visit(RepeatNode node, INfaVM code)
        {
            for (int i = node.MinCount; i != 0; --i)
            {
                node.Inner.Accept(this, code);
            }

            if (node.MaxCount == int.MaxValue)
            {
                var start = code.Labels.Generate();
                var end = code.Labels.Generate();

                code.Label(start);
                code.Fork(end.GetRef());
                node.Inner.Accept(this, code);
                code.Jmp(start.GetRef());
                code.Label(end);
            }
            else
            {
                int optionalCount = node.MaxCount - node.MinCount;
                while (optionalCount-- != 0)
                {
                    var end = code.Labels.Generate();
                    code.Fork(end.GetRef());
                    node.Inner.Accept(this, code);
                    code.Label(end);
                }
            }

            return code;
        }
    }
}
