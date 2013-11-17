using IronText.Framework;
using IronText.Framework.Reflection;
using IronText.Lib.Ctem;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Generates IL code for creating <see cref="EbnfGrammar"/> instance 
    /// </summary>
    public class GrammarSerializer
    {
        private EbnfGrammar grammar;

        public GrammarSerializer(EbnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            var result = emit.Locals.Generate();
            result.Name = "grammar";

            var parts = emit.Locals.Generate();
            emit
                .Local(result, emit.Types.Import(typeof(EbnfGrammar)))
                .Local(parts, emit.Types.Import(typeof(int[])))

                .Newobj(() => new EbnfGrammar())
                .Stloc(result.GetRef())
                ;

            for (int token = 0; token != grammar.SymbolCount; ++token)
            {
                if (!grammar.IsPredefined(token))
                {
                    emit
                        .Ldloc(result.GetRef())
                        .Ldstr(new QStr(grammar.SymbolName(token)))
                        .Ldc_I4((int)grammar.GetTokenCategories(token))
                        .Call((EbnfGrammar g, string name, TokenCategory c) => g.DefineToken(name, c))
                        .Pop()
                        ;
                }
            }

            foreach (var rule in grammar.Productions)
            {
                if (rule.Outcome == EbnfGrammar.AugmentedStart)
                {
                    // Start rule is defined automatically when first token is defined
                    continue;
                }

                emit
                    .Ldc_I4(rule.Pattern.Length)
                    .Newarr(emit.Types.Int32)
                    .Stloc(parts.GetRef())
                    ;

                int i = 0;
                foreach (int part in rule.Pattern)
                {
                    emit
                        .Ldloc(parts.GetRef())
                        .Ldc_I4(i)
                        .Ldc_I4(part)
                        .Stelem_I4()
                        ;
                    ++i;
                }

                emit
                    .Ldloc(result.GetRef())
                    .Ldc_I4(rule.Outcome)
                    .Ldloc(parts.GetRef())
                    .Call((EbnfGrammar g, int l, int[] p) => g.DefineProduction(l, p))
                    .Pop()
                    ;
            }

            for (int token = 2; token != grammar.SymbolCount; ++token)
            {
                if (!grammar.IsTerminal(token))
                {
                    continue;
                }
                var precedence = grammar.GetTermPrecedence(token);
                if (precedence == null)
                {
                    continue;
                }

                emit
                    .Ldloc(result.GetRef())
                    .Ldc_I4(token)
                        .Ldc_I4(precedence.Value)
                        .Ldc_I4((int)precedence.Assoc)
                    .Newobj((int _prec, Associativity _assoc) => new Precedence(_prec, _assoc))
                    .Call((EbnfGrammar g, int t, Precedence p) => g.SetTermPrecedence(t, p))
                    ;
            }

            emit
                .Ldloc(result.GetRef())
                .Call((EbnfGrammar g) => g.Freeze())
                ;
            return emit.Ldloc(result.GetRef());
        }
    }
}
