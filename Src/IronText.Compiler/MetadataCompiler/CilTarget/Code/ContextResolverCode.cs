using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.Shared;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    class ContextResolverCode : IContextResolverCode
    {
        private readonly EmitSyntax         emit;
        private readonly Pipe<EmitSyntax>   ldRootContext;
        private ProductionContextBinding[]     localContexts;
        private Pipe<EmitSyntax>            ldLookback;

        public ContextResolverCode(
            EmitSyntax          emit,
            Pipe<EmitSyntax>    ldRootContext,
            Pipe<EmitSyntax>    ldLookback,
            LanguageData        data,
            ProductionContextBinding[] localContexts = null)
        {
            this.emit = emit;
            this.ldRootContext   = ldRootContext;
            this.ldLookback      = ldLookback;
            this.RootContextType = data.Grammar.Joint.The<CilGrammarSource>().DefinitionType;
            this.localContexts   = localContexts;
        }

        public Type RootContextType { get; set; }

        public void LdContextOfType(Type contextType)
        {
            if (localContexts != null && localContexts.Length != 0)
            {
                var locals =
                    (from lc in localContexts
                     let binding = lc.Consumer.Joint.Get<CilContextConsumer>()
                     where binding != null && contextType == binding.ContextType
                     select lc)
                    .ToArray();

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
                                if (LdGlobalContext(contextType))
                                {
                                    il.Br(END);
                                }
                                else
                                {
                                    il
                                        .Ldstr(new QStr("Internal error: missing global context : " + contextType.FullName))
                                        .Newobj((string msg) => new Exception(msg))
                                        .Throw()
                                        ;
                                }
                            }
                            else
                            {
                                var lc = locals[value];
                                var provider = lc.Provider.Joint.The<CilContextProvider>();
                                var path = provider.GetGetterPath(contextType);
                                if (LdCallPath(path, il2 => il2
                                    // Lookback for getting parent instance
                                                .Do(ldLookback)
                                                .Ldc_I4(lc.ProviderStackLookback)
                                                .Call((IStackLookback<Msg> lb, int backOffset)
                                                        => lb.GetValueAt(backOffset))
                                                .Ldfld((Msg msg) => msg.Value)))
                                {
                                    il.Br(END);
                                }
                                else
                                {
                                    throw new InvalidOperationException(
                                        "Internal error: incorect local context data");
                                }
                            }
                        });

                    emit.Label(END.Def);
                    return;
                }
            }
            
            if (!LdGlobalContext(contextType))
            {
                throw new InvalidOperationException(
                    "Context type '" + contextType.FullName + "' is not accessible.");
            }
        }

        private bool LdGlobalContext(Type type)
        {
            return LdRelativeContext(RootContextType, type, ldRootContext); 
        }

        private bool LdRelativeContext(Type fromType, Type toType, Pipe<EmitSyntax> ldFromContext)
        {
            var path = new CilContextBrowser(fromType).GetGetterPath(toType);
            return LdCallPath(path, ldFromContext);
        }

        private bool LdCallPath(IEnumerable<MethodInfo> path, Pipe<EmitSyntax> ldFrom)
        {
            if (path == null)
            {
                return false;
            }

            // Load start value
            emit.Do(ldFrom);

            foreach (var getter in path)
            {
                emit.Call(getter);
            }

            return true;
        }
    }
}
