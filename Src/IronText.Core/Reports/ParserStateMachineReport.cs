﻿using IronText.Collections;
using IronText.Reflection;
using IronText.Reflection.Reporting;
using IronText.Runtime;
using System.IO;
using System.Linq;
using System.Text;

namespace IronText.Reports
{
    public class ParserStateMachineReport : IReport
    {
        const string Indent = "  ";
        private readonly string fileName;

        public ParserStateMachineReport(string fileName)
        {
            this.fileName = fileName;
        }
        public void Build(IReportData data)
        {
            string path = Path.Combine(data.DestinationDirectory, fileName);

            var conflicts = data.ParserAutomata.Conflicts;

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

                PrintSemanticBindings(data, writer);
            }
        }

        private void PrintTransitions(IReportData data, StreamWriter output)
        {
            string title = "Language: " + data.Source.FullLanguageName;

            output.WriteLine(title);
            output.WriteLine();
            output.WriteLine("Grammar:");
            output.Write(data.Grammar);
            output.WriteLine();

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
                    var symbol = data.Grammar.Symbols[transition.Token];

                    var decision = transition.Decisions;

                    if (decision ==null)
                    {
                        continue;
                    }

                    output.Write(Indent);
                    output.Write(symbol.Name);
                    output.Write("             ");

                    if (decision.IsAmbiguous)
                    {
                        output.Write(Indent);
                        output.WriteLine("conflict {");
                        foreach (var alternative in decision.Alternatives())
                        {
                            PrintDecision(data, symbol, output, alternative);
                        }

                        output.Write(Indent);
                        output.WriteLine("}");
                        output.WriteLine();
                    }
                    else
                    {
                        PrintDecision(data, symbol, output, decision);
                    }
                }

                output.WriteLine();
            }
        }

        private void PrintDecision(
            IReportData    data,
            Symbol         symbol,
            StreamWriter   output,
            ParserDecision decision)
        {
            foreach (var instruction in decision.Instructions)
            {
                PrintInstruction(output, instruction);
            }
        }

        private void PrintInstruction(StreamWriter output, ParserInstruction action)
        {
            switch (action.Operation)
            {
                case ParserOperation.Fail:
                    output.Write("fail");
                    break;
                case ParserOperation.Shift:
                    output.Write("shift and go to state ");
                    output.Write(action.State);
                    break;
                case ParserOperation.Reduce:
                    output.Write("reduce using rule ");
                    output.Write(action.Production);
                    break;
                case ParserOperation.Accept:
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
            ParserInstruction action,
            StreamWriter output,
            string indent)
        {
            switch (action.Operation)
            {
                case ParserOperation.Shift:
                    output.Write(indent);
                    output.Write("Shift to the state I");
                    output.Write(action.State + "");
                    output.WriteLine(":");
                    DescribeState(data, action.State, output, indent + indent);
                    break;
                case ParserOperation.Reduce:
                    output.Write(indent);
                    output.WriteLine("Reduce on the rule:");
                    output.Write(indent + indent);
                    DescribeRule(data, action.Production, output);
                    output.WriteLine();
                    break;
                case ParserOperation.Accept:
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
            var automata = data.ParserAutomata;
            return DescribeState(data, automata.States[state], output, indent);
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
            IReportData     data,
            IParserDotItem  item,
            StreamWriter    output,
            bool showLookaheads = true)
        {
            var production = item.Production;
            output.Write(production.Outcome.Name);
            output.Write(" ->");
            int i = 0;
            foreach (var symbol in production.Input)
            {
                if (item.Position == i)
                {
                    output.Write(" •");
                }

                output.Write(" ");
                output.Write(symbol.Name);

                ++i;
            }

            if (item.Position == production.InputLength)
            {
                output.Write(" •");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(string.Join(", ", (from la in item.LA select data.Grammar.Symbols[la].Name)));
                output.Write("}");
            }

            return output;
        }

        private static StreamWriter DescribeRule(IReportData data, int ruleId, StreamWriter output)
        {
            var rule = data.Grammar.Productions[ruleId];

            output.Write(rule.Outcome.Name);
            output.Write(" ->");

            foreach (var symbol in rule.Input)
            {
                output.Write(" ");
                output.Write(symbol.Name);
            }

            return output;
        }

        private void PrintSemanticBindings(IReportData data, StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("Semantic Bindings:");
            writer.WriteLine();

            int i = 0;
            foreach (var semanticBinding in data.SemanticBindings)
            {
                writer.WriteLine("{0}: ", ++i);
                    
                writer.WriteLine(
                    "    {{ {0} }}",
                    semanticBinding.ProvidingProduction.DebugProductionText);

                writer.WriteLine(
                    "    =({0})=>",
                    GetSemanticName(semanticBinding.Reference.UniqueName));

                writer.WriteLine(
                    "    {{ {0} }}",
                    semanticBinding.ConsumingProduction.DebugProductionText);
            }
        }

        private string GetSemanticName(string name)
        {
            const int MaxLength = 20;

            if (name.Length < MaxLength)
            {
                return name;
            }

            var commaIndex = name.IndexOf(',');
            if (commaIndex > 0)
            {
                return name.Substring(0, commaIndex);
            }

            return name.Substring(0, MaxLength) + "...";
        }
    }
}
