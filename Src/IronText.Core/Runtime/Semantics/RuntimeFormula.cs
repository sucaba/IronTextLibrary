using System;

namespace IronText.Runtime.Semantics
{
    [Serializable]
    public class RuntimeFormula
    {
        private readonly RuntimeReference       lhe;
        private readonly RuntimeReference[]     formalArgs;
        private readonly Func<object[], object> body;

        public RuntimeFormula(
            RuntimeReference       lhe,
            RuntimeReference[]     formalArgs, 
            Func<object[], object> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            this.lhe        = lhe;
            this.formalArgs = formalArgs;
            this.body       = body;
        }

        public void Excecute(IStackLookback<ActionNode> lookback)
        {
            var args = new object[formalArgs.Length];
            for (int i = formalArgs.Length; i != 0; )
            {
                --i;
                var reference = formalArgs[i];
                var node = lookback.GetNodeAt(reference.Offset);
                args[i] = GetValue(node, reference);
            }

            object outcome = body(args);
            var lheNode = lookback.GetNodeAt(lhe.Offset);
            SetValue(lheNode, lhe, outcome);
        }

        private static object GetValue(ActionNode node, RuntimeReference reference)
        {
            switch (reference.Kind)
            {
                case RuntimePropertyKind.Synthesized:
                    return node.GetSynthesizedProperty(reference.Index);
                case RuntimePropertyKind.Inherited:
                    return node.GetInheritedStateProperty(reference.Index);
                default:
                    throw new ArgumentException(nameof(reference));
            }
        }

        private static void SetValue(ActionNode node, RuntimeReference reference, object value)
        {
            switch (reference.Kind)
            {
                case RuntimePropertyKind.Synthesized:
                    node.SetSynthesizedProperty(reference.Index, value);
                    break;
                case RuntimePropertyKind.Inherited:
                    node.SetInheritedStateProperty(reference.Index, value);
                    break;
                default:
                    throw new ArgumentException(nameof(reference));
            }
        }
    }
}
