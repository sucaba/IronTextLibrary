using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Extensibility;

namespace IronText.Framework
{
    public class DescribeParserStateMachineAttribute : LanguageMetadataAttribute
    {
        const string Indent = "  ";
        private readonly string fileName;

        public DescribeParserStateMachineAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        public override IEnumerable<ReportBuilder> GetReportBuilders()
        {
            return new ReportBuilder[] { WriteDocFiles };
        }

        private void WriteDocFiles(IReportData data)
        {
            var automata = data.ParserAutomata;

            string path = Path.Combine(data.DestinationDirectory, fileName);

            var conflicts = automata.Conflicts;

            using (var writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                if (conflicts.Count != 0)
                {
                    writer.WriteLine("Found {0} conflicts", conflicts.Count);

                    foreach (var conflict in conflicts)
                    {
                        ReportConflict(data, conflict, writer);
                    }
                }

                PrintTransitions(data, writer);
            }
        }

        private void PrintTransitions(IReportData data, StreamWriter output)
        {
            string title = "Language: " + data.Name.FullName;

            output.WriteLine(title);
            output.WriteLine();
            output.WriteLine("Grammar:");
            output.Write(data.Grammar);
            output.WriteLine();

            int stateCount = data.ParserAutomata.States.Count;

            foreach (var state in data.ParserAutomata.States)
            {
                output.Write("State ");
                output.Write(state.Index);
                output.WriteLine();
                output.WriteLine();
                DescribeState(data, state, output, Indent);
                output.WriteLine();

                foreach (var transition in state.Transitions)
                {
                    var actions = transition.Actions;
                    int count = actions.Count();

                    if (count == 0)
                    {
                    }
                    else if (count > 1)
                    {
                        output.Write(Indent);
                        output.WriteLine("conflict {");
                        foreach (var action in actions)
                        {
                            PrintAction(data, transition.Token, output, action);
                        }

                        output.Write(Indent);
                        output.WriteLine("}");
                        output.WriteLine();
                    }
                    else
                    {
                        PrintAction(data, transition.Token, output, actions.Single());
                    }
                }

                output.WriteLine();
            }
        }

        private void PrintAction(IReportData data, int token, StreamWriter output, ParserAction action)
        {
            if (action == null || action.Kind == ParserActionKind.Fail)
            {
                return;
            }

            output.Write(Indent);
            output.Write(data.Grammar.SymbolName(token));
            output.Write("             ");
            switch (action.Kind)
            {
#if SWITCH_FEATURE
                case ParserActionKind.Switch:
                    output.Write("switch to the external token ");
                    output.Write(action.ExternalToken);
                    break;
#endif
                case ParserActionKind.Shift:
                    output.Write("shift and go to state ");
                    output.Write(action.State);
                    break;
                case ParserActionKind.Reduce:
                    output.Write("reduce using rule ");
                    output.Write(action.Rule);
                    break;
                case ParserActionKind.ShiftReduce:
                    output.Write("shift-reduce using rule ");
                    output.Write(action.Rule);
                    break;
                case ParserActionKind.Accept:
                    output.WriteLine("accept");
                    break;
            }

            output.WriteLine();
        }

        private void ReportConflict(IReportData data, ParserConflictInfo conflict, StreamWriter message)
        {
            const string Indent = "  ";

            message.WriteLine(new string('-', 50));
            message.Write("Conflict on token ");
            message.Write(data.Grammar.SymbolName(conflict.Token));
            message.Write(" between actions in state #");
            message.Write(conflict.State + "");
            message.WriteLine(":");
            DescribeState(data, conflict.State, message, Indent).WriteLine();
            for (int i = 0; i != conflict.Actions.Count; ++i)
            {
                message.WriteLine("Action #{0}", i);
                DescribeAction(data, conflict.Actions[i], message, Indent);
            }

            message.WriteLine(new string('-', 50));
        }

        private StreamWriter DescribeAction(
            IReportData data,
            ParserAction action,
            StreamWriter output,
            string indent)
        {
            switch (action.Kind)
            {
#if SWITCH_FEATURE
                case ParserActionKind.Switch:
                    output.Write(indent);
                    output.Write("Swith to the external token ");
                    output.Write(data.Grammar.TokenName(action.ExternalToken));
                    output.WriteLine(":");
                    break;
#endif
                case ParserActionKind.Shift:
                    output.Write(indent);
                    output.Write("Shift to the state I");
                    output.Write(action.State + "");
                    output.WriteLine(":");
                    DescribeState(data, action.State, output, indent + indent);
                    break;
                case ParserActionKind.ShiftReduce:
                    output.Write(indent);
                    output.WriteLine("Shift-Reduce on the rule:");
                    output.Write(indent + indent);
                    DescribeRule(data, action.Rule, output);
                    output.WriteLine();
                    break;
                case ParserActionKind.Reduce:
                    output.Write(indent);
                    output.WriteLine("Reduce on the rule:");
                    output.Write(indent + indent);
                    DescribeRule(data, action.Rule, output);
                    output.WriteLine();
                    break;
                case ParserActionKind.Accept:
                    output.Write(indent);
                    output.WriteLine("Accept.");
                    break;
            }

            return output;
        }

        private static StreamWriter DescribeState(
            IReportData data,
            int state,
            StreamWriter output,
            string indent)
        {
            return DescribeState(data, data.ParserAutomata.States[state], output, indent);
        }

        private static StreamWriter DescribeState(
            IReportData data,
            IParserState state,
            StreamWriter output,
            string indent)
        {
            foreach (var item in state.DotItems)
            {
                output.Write(indent);
                DescribeItem(data, item, output);
                output.WriteLine();
            }

            return output;
        }

        private static StreamWriter DescribeItem(
            IReportData data,
            IParserDotItem item,
            StreamWriter output,
            bool showLookaheads = true)
        {
            var rule = item.Rule;
            output.Write(data.Grammar.SymbolName(rule.Left));
            output.Write(" ->");
            for (int i = 0; i != rule.Parts.Length; ++i)
            {
                if (item.Position == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(data.Grammar.SymbolName(rule.Parts[i]));
            }

            if (item.Position == rule.Parts.Length)
            {
                output.Write(" •");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(string.Join(", ", item.Lookaheads.Select(data.Grammar.SymbolName)));
                output.Write("}");
            }

            return output;
        }

        private static StreamWriter DescribeRule(IReportData data, int ruleId, StreamWriter output)
        {
            var rule = data.Grammar.Rules[ruleId];

            output.Write(data.Grammar.SymbolName(rule.Left));
            output.Write(" ->");

            for (int i = 0; i != rule.Parts.Length; ++i)
            {
                output.Write(" ");
                output.Write(data.Grammar.SymbolName(rule.Parts[i]));
            }

            return output;
        }

        
    }
}
