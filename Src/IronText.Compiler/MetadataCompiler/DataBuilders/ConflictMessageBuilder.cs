using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Logging;
using IronText.Reporting;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class ConflictMessageBuilder : IReport
    {
        private IReportData data;
        private readonly ILogging logging;

        public ConflictMessageBuilder(ILogging logging)
        {
            this.logging = logging;
        }

        public void Build(IReportData data)
        {
            this.data = data;
            Write();
        }

        private void Write()
        {
            var conflicts = data.ParserAutomata.GetConflicts().ToArray();
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Origin   = data.Source.GrammarOrigin,
                    Message  = string.Format(
                                    "Found {0} parser conflict{1}.",
                                    conflicts.Length,
                                    conflicts.Length == 1 ? "" : "s")
                });

            var output = new StringBuilder();

            foreach (var conflict in conflicts)
            {
                WriteConflictEntry(conflict, logging);
            }

            // Write fixing advice
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Warning,
                    Origin   = data.Source.GrammarOrigin,
                    Message  = string.Format(
                        "Consider using [{0}({1}.{2})] or changing token and/or rule precedences to fix errors.",
                        typeof(LanguageAttribute).Name,
                        typeof(RuntimeOptions).Name,
                        Enum.GetName(typeof(RuntimeOptions), RuntimeOptions.AllowNonDeterministic))
                });
        }

        private void WriteConflictEntry(ParserConflict conflict, ILogging logging)
        {
            using (var writer = new StringWriter())
            using (var message = new IndentedTextWriter(writer, "  "))
            {
                var symbol = conflict.Transition.Symbol;

                message.Write("Conflict on token ");
                message.Write(symbol);
                message.Write(" between actions in state #");
                message.Write(conflict.State.Index + "");
                message.WriteLine(":");

                ++message.Indent;
                DescribeState(message, conflict.State);
                int i = 0;
                foreach (var decision in conflict.Transition.AlternateDecisions)
                {
                    message.WriteLine("Action #{0}:", i + 1);
                    DescribeAction(message, decision);
                }

                --message.Indent;

                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Origin   = data.Source.GrammarOrigin,
                        Message  = writer.ToString()
                    });
            }
        }

        private void DescribeState(IndentedTextWriter message, int state)
        {
            DescribeState(message, data.ParserAutomata.States[state]);
        }

        private void DescribeState(IndentedTextWriter message, IParserState state)
        {
            foreach (var item in state.DotItems)
            {
                DescribeItem(message, item);
                message.WriteLine();
            }
        }

        private void DescribeItem(
            IndentedTextWriter output,
            IParserDotItem item,
            bool showLookaheads = true)
        {
            output.Write(item.Outcome);
            output.Write(" ->");
            var input = item.Input;
            for (int i = 0; i != input.Length; ++i)
            {
                if (item.Position == i)
                {
                    output.Write(" .>");
                }

                output.Write(" ");
                output.Write(input[i]);
            }

            if (item.Position == input.Length)
            {
                output.Write(" .>");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(string.Join(", ", item.LA));
                output.Write("}");
            }
        }

        private void DescribeAction(IndentedTextWriter message, IParserDecision action)
        {
            message.Write(action.ActionText);
            message.Write(action.NextState.Index + "");
            message.WriteLine(":");
            ++message.Indent;
            DescribeState(message, action.NextState);
            --message.Indent;

            message.WriteLine();
        }

        private void DescribeRule(IndentedTextWriter output, int ruleId)
        {
            var prod = data.Grammar.Productions[ruleId];

            output.Write(prod.Outcome.Name);
            output.Write(" ->");

            for (int i = 0; i != prod.Input.Length; ++i)
            {
                output.Write(" ");
                output.Write(prod.Input[i].Name);
            }
        }
    }
}
