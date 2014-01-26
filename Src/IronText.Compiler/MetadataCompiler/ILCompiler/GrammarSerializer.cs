using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IronText.Framework;
using IronText.Collections;
using IronText.Reflection;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Misc;

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
            var resultVar   = emit.Locals.Generate("result");
            var partsVar    = emit.Locals.Generate("parts");
            var symbolVar   = emit.Locals.Generate("symbol").GetRef();
            var intArrayVar = emit.Locals.Generate("intArray").GetRef();
            emit
                .Local(resultVar,       emit.Types.Import(typeof(EbnfGrammar)))
                .Local(partsVar,        emit.Types.Import(typeof(int[])))
                .Local(symbolVar.Def,   emit.Types.Import(typeof(SymbolBase)))
                .Local(intArrayVar.Def, emit.Types.Array(emit.Types.Int32))

                .Newobj(() => new EbnfGrammar())
                .Stloc(resultVar.GetRef())
                ;

            foreach (var symbol in grammar.Symbols)
            {
                if (symbol.IsPredefined)
                {
                    continue;
                }

                if (symbol is Symbol)
                {
                    var determSymbol = (Symbol)symbol;
                    emit
                        .Ldstr(new QStr(symbol.Name))
                        .Newobj((string name) => new Symbol(name))
                        .Stloc(symbolVar)
                        ;

                    if (symbol.Categories != SymbolCategory.None)
                    {
                        emit
                            .Ldloc(symbolVar)
                            .Ldc_I4((int)symbol.Categories)
                            .Stprop((Symbol s) => s.Categories)
                            ;
                    }

                    if (symbol.Precedence != null)
                    {
                        var precedence = symbol.Precedence;

                        emit
                            .Ldloc(symbolVar)

                            .Ldc_I4(precedence.Value)
                            .Ldc_I4((int)precedence.Assoc)
                            .Newobj((int _prec, Associativity _assoc) => new Precedence(_prec, _assoc))

                            .Stprop((Symbol s) => s.Precedence)
                            ;
                    }
                }
                else if (symbol is AmbiguousSymbol)
                {
                    var ambSymbol = (AmbiguousSymbol)symbol;

                    emit
                        .Ldc_I4(ambSymbol.Tokens.Count)
                        .Newarr(emit.Types.Int32)
                        .Stloc(intArrayVar)
                        ;

                    for (int i = 0; i != ambSymbol.Tokens.Count; ++i)
                    {
                        emit
                            .Ldloc(intArrayVar)
                            .Ldc_I4(i)
                            .Ldc_I4(ambSymbol.Tokens[i])
                            .Stelem_I4()
                            ;
                    }

                    emit
                        .Ldc_I4(ambSymbol.MainToken)
                        .Ldloc(intArrayVar)
                        .Newobj((int main, int[] tokens) => new AmbiguousSymbol(main, tokens))
                        .Stloc(symbolVar)
                        ;
                }
                else
                {
                    throw new InvalidOperationException("Internal error: unknown symbol type.");
                }

                emit
                    .Ldloc(resultVar.GetRef())
                    .Ldprop((EbnfGrammar g) => g.Symbols)
                    .Ldloc(symbolVar)
                    .Call((SymbolCollection coll, Symbol sym) => coll.Add(sym))
                    .Pop()
                    ;
            }

            if (grammar.Start != null)
            {
                emit
                    .Ldloc(resultVar.GetRef())
                    .Dup()
                    .Ldprop((EbnfGrammar g) => g.Symbols)
                    .Ldc_I4(grammar.Start.Index)
                    .Call((SymbolCollection coll, int index) => coll[index])
                    .Stprop((EbnfGrammar g) => g.Start);
            }

            foreach (var production in grammar.Productions)
            {
                if (production.OutcomeToken == EbnfGrammar.AugmentedStart)
                {
                    // Start rule is defined automatically when first token is defined
                    continue;
                }

                emit
                    .Ldc_I4(production.PatternTokens.Length)
                    .Newarr(emit.Types.Int32)
                    .Stloc(partsVar.GetRef())
                    ;

                int i = 0;
                foreach (int part in production.PatternTokens)
                {
                    emit
                        .Ldloc(partsVar.GetRef())
                        .Ldc_I4(i)
                        .Ldc_I4(part)
                        .Stelem_I4()
                        ;
                    ++i;
                }

                emit
                    .Ldloc(resultVar.GetRef())
                    .Ldprop((EbnfGrammar g) => g.Productions)
                    .Ldc_I4(production.OutcomeToken)
                    .Ldloc(partsVar.GetRef())
                    .Call((ProductionCollection prods, int l, IEnumerable<int> p) => prods.Define(l, p))
                    .Pop()
                    ;
            }

            return emit.Ldloc(resultVar.GetRef());
        }
    }
}
