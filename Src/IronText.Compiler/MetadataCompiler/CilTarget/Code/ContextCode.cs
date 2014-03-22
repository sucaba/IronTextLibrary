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
    interface IContextCode
    {
        void LdContext(string contextName);
    }

    class ContextCode : IContextCode
    {
        private readonly EmitSyntax       emit;
        private readonly Pipe<EmitSyntax> ldGlobalContextProvider;
        private readonly Pipe<EmitSyntax> ldLookback;
        private readonly SematnicContextProvider  contextProvider;
        private readonly LocalContextBinding[] localContexts;

        public ContextCode(
            EmitSyntax       emit,
            Pipe<EmitSyntax> ldGlobalContextProvider,
            Pipe<EmitSyntax> ldLookback,
            LanguageData     data,
            SematnicContextProvider  contextProvider,
            LocalContextBinding[] localContexts = null)
        {
            this.emit = emit;
            this.ldGlobalContextProvider   = ldGlobalContextProvider;
            this.ldLookback      = ldLookback;
            this.contextProvider = contextProvider;
            this.localContexts   = localContexts;
        }

        public void LdContext(string contextName)
        {
            var contextRef = new SemanticContextRef(contextName);

            if (localContexts != null && localContexts.Length != 0)
            {
                var locals = localContexts.Where(lc => lc.Consumer.Equals(contextRef)).ToArray();
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
                    Expression<Action<IStackLookback<Msg>>> lookback = lb => lb.GetParentState();
                    switchGenerator.Build(
                        emit,
                        new Pipe<EmitSyntax>(il => il.Do(ldLookback).Call<IStackLookback<Msg>>(lookback)),
                        (il, value) =>
                        {
                            il.Label(labels[value].Def);

                            if (value == locals.Length)
                            {
                                if (LdGlobalContext(contextName))
                                {
                                    il.Br(END);
                                }
                                else
                                {
                                    il
                                        .Ldstr(new QStr("Internal error: missing global context : " + contextRef.UniqueName))
                                        .Newobj((string msg) => new Exception(msg))
                                        .Throw()
                                        ;
                                }
                            }
                            else
                            {
                                var lc = locals[value];
                                var provider = lc.Provider.Joint.The<CilContextProvider>();
                                var context = provider.GetContext(contextName);
                                context.Ld(
                                    il,
                                    il2 => il2
                                        // Lookback for getting parent instance
                                        .Do(ldLookback)
                                        .Ldc_I4(lc.StackLookback)
                                        .Call((IStackLookback<Msg> lb, int backOffset)
                                                => lb.GetValueAt(backOffset))
                                        .Ldfld((Msg msg) => msg.Value));
                                il.Br(END);
                            }
                        });

                    emit.Label(END.Def);
                    return;
                }
            }
            
            if (!LdGlobalContext(contextName))
            {
                throw new InvalidOperationException(
                    "Context '" + contextName + "' is not accessible.");
            }
        }

        private bool LdGlobalContext(string contextName)
        {
            var contextRef = new SemanticContextRef(contextName);
            var context = this.contextProvider.Resolve(contextRef);
            if (context == null)
            {
                return false;
            }

            var binding = context.Joint.The<CilContext>();
            binding.Ld(emit, ldGlobalContextProvider);
            return true;
        }
    }
}
