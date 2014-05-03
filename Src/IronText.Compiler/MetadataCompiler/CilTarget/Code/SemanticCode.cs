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
    interface ISemanticCode
    {
        void LdSemantic(string name);
    }

    class SemanticCode : ISemanticCode
    {
        private readonly EmitSyntax            emit;
        private readonly Pipe<EmitSyntax>      ldGlobalScope;
        private readonly Pipe<EmitSyntax>      ldLookback;
        private readonly SemanticScope         globals;
        private readonly LocalSemanticBinding[] localSemanticBindings;

        public SemanticCode(
            EmitSyntax             emit,
            Pipe<EmitSyntax>       ldGlobalScope,
            Pipe<EmitSyntax>       ldLookback,
            LanguageData           data,
            SemanticScope          globals,
            LocalSemanticBinding[] localSemanticBindings = null)
        {
            this.emit                  = emit;
            this.ldGlobalScope         = ldGlobalScope;
            this.ldLookback            = ldLookback;
            this.globals               = globals;
            this.localSemanticBindings = localSemanticBindings;
        }

        public void LdSemantic(string name)
        {
            if (name == null)
            {
                return;
            }

            var reference = new SemanticRef(name);

            if (localSemanticBindings != null && localSemanticBindings.Length != 0)
            {
                var locals = localSemanticBindings.Where(lc => lc.ConsumerRef.Equals(reference)).ToArray();
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
                                if (LdGlobal(reference))
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
                                SemanticValue val = lc.Locals.Resolve(reference);
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
                    return;
                }
            }
            
            if (!LdGlobal(reference))
            {
                throw new InvalidOperationException(
                    "Semantic value '" + name + "' is not accessible.");
            }
        }

        private bool LdGlobal(SemanticRef reference)
        {
            var value = globals.Resolve(reference);
            if (value == null)
            {
                return false;
            }

            var binding = value.Joint.The<CilSemanticValue>();
            binding.Ld(emit, ldGlobalScope);
            return true;
        }
    }
}
