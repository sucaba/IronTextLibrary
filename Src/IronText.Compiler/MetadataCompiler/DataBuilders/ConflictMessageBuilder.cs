using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Logging;
using IronText.Reflection.Reporting;
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

        public void Build(IReportData reportData)
        {
            data = reportData;
            Write();
        }

        private void Write()
        {
            var conflicts = data.ParserAutomata.Conflicts;
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Origin   = data.Source.GrammarOrigin,
                    Message  = string.Format(
                                    "Found {0} parser conflict{1}.",
                                    conflicts.Count,
                                    conflicts.Count == 1 ? "" : "s")
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

        private void WriteConflictEntry(ParserConflictInfo conflict, ILogging logging)
        {
            using (var writer = new StringWriter())
            using (var message = new IndentedTextWriter(writer, "  "))
            {
                var symbol = data.Grammar.Symbols[conflict.Token];

                message.Write("Conflict on token ");
                message.Write(symbol.Name);
                message.Write(" between actions in state #");
                message.Write(conflict.State + "");
                message.WriteLine(":");

                ++message.Indent;
                DescribeState(message, conflict.State);
                for (int i = 0; i != conflict.Actions.Count; ++i)
                {
                    message.WriteLine("Action #{0}:", i + 1);
                    DescribeAction(message, conflict.Actions[i]);
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
            var prod = item.Production;
            output.Write(prod.Outcome.Name);
            output.Write(" ->");
            for (int i = 0; i != prod.Input.Length; ++i)
            {
                if (item.Position == i)
                {
                    output.Write(" .>");
                }

                output.Write(" ");
                output.Write(prod.Input[i].Name);
            }

            if (item.Position == prod.Input.Length)
            {
                output.Write(" .>");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(
                    string.Join(
                        ", ",
                        from la in item.LA
                        select data.Grammar.Symbols[la].Name));
                output.Write("}");
            }
        }

        private void DescribeAction(IndentedTextWriter message, ParserInstruction action)
        {
            switch (action.Operation)
            {
                case ParserOperation.Shift:
                    message.Write("Shift to state #");
                    message.Write(action.State + "");
                    message.WriteLine(":");
                    ++message.Indent;
                    DescribeState(message, action.State);
                    --message.Indent;
                    break;
                case ParserOperation.Reduce:
                    message.WriteLine("Reduce on the rule:");
                    ++message.Indent;
                    DescribeRule(message, action.Production);
                    --message.Indent;
                    break;
                case ParserOperation.Accept:
                    message.Write("Accept.");
                    break;
            }

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
