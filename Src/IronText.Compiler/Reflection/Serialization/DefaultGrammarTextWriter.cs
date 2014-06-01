using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using IronText.Misc;

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
            output.WriteLine("symbols       : {0}", grammar.Symbols.Count);
            output.WriteLine("terminals     : {0}", grammar.Symbols.Where(s => s.IsTerminal).Count());
            output.WriteLine("non-terminals : {0}", grammar.Symbols.Where(s => !s.IsTerminal).Count());
            output.WriteLine("productions(+): {0}", grammar.Productions.Count(p => !p.IsDeleted));
            output.WriteLine("productions(-): {0}", grammar.Productions.Count(p => p.IsDeleted));
            output.WriteLine("mergers       : {0}", grammar.Mergers.Count);
            output.WriteLine("matchers      : {0}", grammar.Matchers.Count);
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

                if (prod.ExplicitPrecedence != null)
                {
                    if (prod.IsDeleted)
                    {
                        output.Write("// *deleted*:  ");
                    }

                    output.WriteLine(
                        "[Precedence({0},{1})]",
                        prod.EffectivePrecedence.Value,
                        Enum.GetName(typeof(Associativity), prod.EffectivePrecedence.Assoc));
                }

                if (prod.IsDeleted)
                {
                    output.Write("// *deleted*:  ");
                }

                InternalWriteComponent(output, prod);

                output.WriteLine(";");
                output.WriteLine();
            }
        }

        private static void InternalWriteComponent(
            IndentedTextWriter   output,
            IProductionComponent component,
            bool                 showParenthes = false)
        {
            Symbol     symbol;
            Production prod;

            switch (component.Match(out symbol, out prod))
            {
                case 0:
                    {
                        output.Write(symbol.Name);
                        break;
                    }
                case 1:
                    {
                        if (showParenthes)
                        {
                            output.Write('(');
                        }

                        output.Write("{0} :", prod.Outcome.Name);
                        if (prod.Pattern.Length == 0)
                        {
                            output.Write(" /*empty*/");
                        }
                        else
                        {
                            foreach (var child in prod.Components)
                            {
                                output.Write(' ');
                                InternalWriteComponent(output, child, true);
                            }
                        }

                        if (showParenthes)
                        {
                            output.Write(')');
                        }

                        break;
                    }
            }
        }

        private void WriteMatchers(Grammar grammar, IndentedTextWriter output)
        {
            output.WriteLine("scanner");
            output.WriteLine("{");
            ++output.Indent;

            foreach (var matcher in grammar.Matchers)
            {
                output.WriteLine(
                    "{0}: /{1}/;",
                    Name(matcher.Outcome),
                    matcher.Pattern);
            }

            --output.Indent;
            output.WriteLine("}");
            output.WriteLine();
        }

        private static string Name(SymbolBase symbol)
        {
            return symbol == null ? "$skip" : symbol.Name; 
        }
    }
}
