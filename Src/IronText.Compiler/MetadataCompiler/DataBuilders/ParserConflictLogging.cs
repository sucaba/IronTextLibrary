using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Automata.Lalr1;
using IronText.Framework;
using IronText.Extensibility;
using System.IO;
using System.CodeDom.Compiler;

namespace IronText.MetadataCompiler
{
    class ConflictMessageBuilder
    {
        private readonly IReportData data;

        public ConflictMessageBuilder(IReportData reportData)
        {
            this.data = reportData;
        }

        public void Write(ILogging logging)
        {
            var conflicts = data.ParserAutomata.Conflicts;
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Member   = data.Name.DefinitionType,
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
                    Member   = data.Name.DefinitionType,
                    Message  = string.Format(
                        "Consider using [{0}({1}.{2})] or changing token and/or rule precedences to fix errors.",
                        typeof(LanguageAttribute).Name,
                        typeof(LanguageFlags).Name,
                        Enum.GetName(typeof(LanguageFlags), LanguageFlags.AllowNonDeterministic))
                });
        }

        private void WriteConflictEntry(ParserConflictInfo conflict, ILogging logging)
        {
            using (var writer = new StringWriter())
            using (var message = new IndentedTextWriter(writer, "  "))
            {
                message.Write("Conflict on token ");
                message.Write(data.Grammar.SymbolName(conflict.Token));
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
                        Member   = data.Name.DefinitionType,
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
            var rule = item.Rule;
            output.Write(data.Grammar.SymbolName(rule.Left));
            output.Write(" ->");
            for (int i = 0; i != rule.Parts.Length; ++i)
            {
                if (item.Position == i)
                {
                    output.Write(" .>");
                }

                output.Write(" ");
                output.Write(data.Grammar.SymbolName(rule.Parts[i]));
            }

            if (item.Position == rule.Parts.Length)
            {
                output.Write(" .>");
            }

            if (showLookaheads)
            {
                output.Write("  |LA = {");
                output.Write(string.Join(", ", item.Lookaheads.Select(data.Grammar.SymbolName)));
                output.Write("}");
            }
        }

        private void DescribeAction(IndentedTextWriter message, ParserAction action)
        {
            switch (action.Kind)
            {
#if SWITCH_FEATURE
                case ParserActionKind.Switch:
                    message.Write("Swith to the external token ");
                    message.Write(data.Grammar.TokenName(action.ExternalToken));
                    message.WriteLine();
                    break;
#endif
                case ParserActionKind.Shift:
                    message.Write("Shift to state #");
                    message.Write(action.State + "");
                    message.WriteLine(":");
                    ++message.Indent;
                    DescribeState(message, action.State);
                    --message.Indent;
                    break;
                case ParserActionKind.ShiftReduce:
                    message.WriteLine("Shift-Reduce on the rule:");
                    ++message.Indent;
                    DescribeRule(message, action.Rule);
                    --message.Indent;
                    break;
                case ParserActionKind.Reduce:
                    message.WriteLine("Reduce on the rule:");
                    ++message.Indent;
                    DescribeRule(message, action.Rule);
                    --message.Indent;
                    break;
                case ParserActionKind.Accept:
                    message.Write("Accept.");
                    break;
            }

            message.WriteLine();
        }

        private void DescribeRule(IndentedTextWriter output, int ruleId)
        {
            var rule = data.Grammar.Rules[ruleId];

            output.Write(data.Grammar.SymbolName(rule.Left));
            output.Write(" ->");

            for (int i = 0; i != rule.Parts.Length; ++i)
            {
                output.Write(" ");
                output.Write(data.Grammar.SymbolName(rule.Parts[i]));
            }
        }
    }
}
