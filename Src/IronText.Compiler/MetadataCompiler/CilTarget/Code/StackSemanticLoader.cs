using System;
using System.Linq;
using System.Linq.Expressions;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class StackSemanticLoader : ISemanticLoader
    {
        private EmitSyntax                      emit;
        private readonly Pipe<EmitSyntax>       ldLookback;
        private readonly StackSemanticBinding[] localSemanticBindings;
        private readonly ISemanticLoader        globals;

        public StackSemanticLoader(ISemanticLoader globals, EmitSyntax emit, Pipe<EmitSyntax> ldLookback, StackSemanticBinding[] localSemanticBindings = null)
        {
            this.globals               = globals;
            this.emit                  = emit;
            this.ldLookback            = ldLookback;
            this.localSemanticBindings = localSemanticBindings == null 
                                       ? new StackSemanticBinding[0] 
                                       : localSemanticBindings
                                            .OfType<StackSemanticBinding>()
                                            .ToArray();
        }

        public bool LdSemantic(SemanticRef reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            if (reference == SemanticRef.None)
            {
                return true;
            }

            if (localSemanticBindings.Length != 0)
            {
                var locals = localSemanticBindings.Where(lc => lc.Reference.Equals(reference)).ToArray();

                if (locals.Length != 0)
                {
                    var END = emit.Labels.Generate().GetRef();
                    var labels = new Ref<Labels>[locals.Length + 1];
                    for (int i = 0; i != labels.Length; ++i)
                    {
                        labels[i] = emit.Labels.Generate().GetRef();
                    }

                    var map =
                        (from i in Enumerable.Range(0, locals.Length)
                         let lc = locals[i]
                         select new IntArrow<int>(lc.StackState, i))
                        .ToArray();

                    var switchGenerator = SwitchGenerator.Sparse(
                                            map,
                                            labels.Length - 1,
                                            new IntInterval(0, short.MaxValue));
                    switchGenerator.Build(
                        emit,
                        il => il
                                .Do(ldLookback)
                                .Call((IStackLookback<ActionNode> lb) => lb.GetParentState()),
                        (il, value) =>
                        {
                            il.Label(labels[value].Def);

                            if (value == locals.Length)
                            {
                                if (globals.LdSemantic(reference))
                                {
                                    il.Br(END);
                                }
                                else
                                {
                                    il
                                        .Ldstr(new QStr("Internal error: missing global value : " + reference.UniqueName))
                                        .Newobj((string msg) => new Exception(msg))
                                        .Throw()
                                        ;
                                }
                            }
                            else
                            {
                                var lc = locals[value];
                                SemanticValue val = lc.Scope.Resolve(reference);
                                CilSemanticValue valBinding = val.Joint.The<CilSemanticValue>();
                                valBinding.Ld(
                                    il,
                                    il2 => il2
                                        // Lookback for getting parent instance
                                        .Do(ldLookback)
                                        .Ldc_I4(lc.StackLookback)
                                        .Call((IStackLookback<ActionNode> lb, int backOffset)
                                                => lb.GetValueAt(backOffset))
                                        .Ldfld((ActionNode msg) => msg.Value));
                                il.Br(END);
                            }
                        });

                    emit.Label(END.Def);
                    return true;
                }
            }
            
            if (!globals.LdSemantic(reference))
            {
                throw new InvalidOperationException(
                    "Semantic value '" + reference + "' is not accessible.");
            }

            return true;
        }
    }
}
