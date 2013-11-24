using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Extensibility;
using IronText.Framework.Reflection;

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

                    var symbol = data.Grammar.Symbols[transition.Token];

                    if (count == 0)
                    {
                    }
                    else if (count > 1)
                    {
                        output.Write(Indent);
                        output.WriteLine("conflict {");
                        foreach (var action in actions)
                        {
                            PrintAction(data, symbol, output, action);
                        }

                        output.Write(Indent);
                        output.WriteLine("}");
                        output.WriteLine();
                    }
                    else
                    {
                        PrintAction(data, symbol, output, actions.Single());
                    }
                }

                output.WriteLine();
            }
        }

        private void PrintAction(IReportData data, SymbolBase symbol, StreamWriter output, ParserAction action)
        {
            if (action == null || action.Kind == ParserActionKind.Fail)
            {
                return;
            }

            output.Write(Indent);
            output.Write(symbol.Name);
            output.Write("             ");
            switch (action.Kind)
            {
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
            var symbol = data.Grammar.Symbols[conflict.Token];

            const string Indent = "  ";

            message.WriteLine(new string('-', 50));
            message.Write("Conflict on token ");
            message.Write(symbol.Name);
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
            var production = item.Rule;
            output.Write(production.OutcomeSymbol.Name);
            output.Write(" ->");
            int i = 0;
            foreach (var symbol in production.PatternSymbols)
            {
                if (item.Position == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(symbol.Name);

                ++i;
            }

            if (item.Position == production.Pattern.Length)
            {
                output.Write(" •");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(string.Join(", ", (from la in item.Lookaheads select data.Grammar.Symbols[la].Name)));
                output.Write("}");
            }

            return output;
        }

        private static StreamWriter DescribeRule(IReportData data, int ruleId, StreamWriter output)
        {
            var rule = data.Grammar.Productions[ruleId];

            output.Write(rule.OutcomeSymbol.Name);
            output.Write(" ->");

            foreach (var symbol in rule.PatternSymbols)
            {
                output.Write(" ");
                output.Write(symbol.Name);
            }

            return output;
        }
    }
}
