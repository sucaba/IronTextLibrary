using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.IL;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Generates IL code for creating <see cref="BnfGrammar"/> instance 
    /// </summary>
    public class GrammarSerializer
    {
        private BnfGrammar grammar;

        public GrammarSerializer(BnfGrammar grammar)
        {
            this.grammar = grammar;
        }

        public EmitSyntax Build(EmitSyntax emit)
        {
            var result = emit.Locals.Generate();
            result.Name = "grammar";

            var parts = emit.Locals.Generate();
            emit
                .Local(result, emit.Types.Import(typeof(BnfGrammar)))
                .Local(parts, emit.Types.Import(typeof(int[])))

                .Newobj(() => new BnfGrammar())
                .Stloc(result.GetRef())
                ;

            for (int token = 0; token != grammar.TokenCount; ++token)
            {
                if (!grammar.IsPredefined(token))
                {
                    emit
                        .Ldloc(result.GetRef())
                        .Ldstr(new QStr(grammar.TokenName(token)))
                        .Ldc_I4((int)grammar.GetTokenCategories(token))
                        .Call((BnfGrammar g, string name, TokenCategory c) => g.DefineToken(name, c))
                        .Pop()
                        ;
                }
            }

            foreach (var rule in grammar.Rules)
            {
                if (rule.Left == BnfGrammar.AugmentedStart)
                {
                    // Start rule is defined automatically when first token is defined
                    continue;
                }

                emit
                    .Ldc_I4(rule.Parts.Length)
                    .Newarr(emit.Types.Int32)
                    .Stloc(parts.GetRef())
                    ;

                int i = 0;
                foreach (int part in rule.Parts)
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
                    .Ldc_I4(rule.Left)
                    .Ldloc(parts.GetRef())
                    .Call((BnfGrammar g, int l, int[] p) => g.DefineRule(l, p))
                    .Pop()
                    ;
            }

            for (int token = 2; token != grammar.TokenCount; ++token)
            {
                if (!grammar.IsTerm(token))
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
                    .Call((BnfGrammar g, int t, Precedence p) => g.SetTermPrecedence(t, p))
                    ;
            }

            emit
                .Ldloc(result.GetRef())
                .Call((BnfGrammar g) => g.Freeze())
                ;
            return emit.Ldloc(result.GetRef());
        }
    }
}
