using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace IronText.Reflection
{
    class DefaultTextGrammarWriter : IGrammarTextWriter
    {
        private const string Indent = "    ";

        public void Write(Grammar grammar, TextWriter output)
        {
            WriteIndented(grammar, new IndentedTextWriter(output, Indent));
        }

        private void WriteIndented(Grammar grammar, IndentedTextWriter output)
        {
            WriteSummaryComment(grammar, output);
            WriteProductions(grammar, output);
            WriteMatchers(grammar, output);
        }

        private static void WriteSummaryComment(Grammar grammar, IndentedTextWriter output)
        {
            output.WriteLine("/*");
            ++output.Indent;
            output.WriteLine("Symbols      : {0}", grammar.Symbols.Count);
            output.WriteLine("Terminals    : {0}", grammar.Symbols.Where(s => s.IsTerminal).Count());
            output.WriteLine("NonTerminals : {0}", grammar.Symbols.Where(s => !s.IsTerminal).Count());
            output.WriteLine("Productions  : {0}", grammar.Productions.Count);
            output.WriteLine("Mergers      : {0}", grammar.Mergers.Count);
            output.WriteLine("Matchers     : {0}", grammar.Matchers.Count);
            --output.Indent;
            output.WriteLine("*/");
            output.WriteLine();
        }

        private static void WriteProductions(Grammar grammar, IndentedTextWriter output)
        {
            output.WriteLine("// Production rules:");
            output.WriteLine();

            foreach (var prod in grammar.Productions)
            {
                output.WriteLine("// {0}:", prod.Index);
                output.Write("{0} :", prod.Outcome.Name);
                if (prod.Pattern.Length == 0)
                {
                    output.Write(" /*empty*/");
                }
                else
                {
                    foreach (var symb in prod.Pattern)
                    {
                        output.Write(' ');
                        output.Write(symb.Name);
                    }
                }

                output.WriteLine(";");
                output.WriteLine();
            }
        }

        private void WriteMatchers(Grammar grammar, IndentedTextWriter output)
        {
            output.WriteLine("// Scanner Conditions: ");
            output.WriteLine();

            foreach (var condition in grammar.Conditions)
            {
                output.WriteLine("condition {0}", condition.Name);
                output.WriteLine("{");
                ++output.Indent;

                foreach (var matcher in condition.Matchers)
                {
                    output.WriteLine("{0} : /{1}/;", Name(matcher.Outcome), matcher.Pattern);
                }

                --output.Indent;
                output.WriteLine("}");
                output.WriteLine();
            }
        }

        private static string Name(SymbolBase symbol)
        {
            return symbol == null ? "" : symbol.Name; 
        }
    }
}
