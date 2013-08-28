using System;
using System.Linq;
using IronText.Algorithm;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Lib.IL;
using IronText.Lib.IL.Generators;
using IronText.Lib.Shared;

namespace IronText.MetadataCompiler
{
    public class ContextResolverCode : IContextResolverCode
    {
        private readonly EmitSyntax         emit;
        private readonly Pipe<EmitSyntax>   ldRootContext;
        private LocalParseContext[]         localContexts;
        private Pipe<EmitSyntax>            ldLookback;

        public ContextResolverCode(
            EmitSyntax          emit,
            Pipe<EmitSyntax>    ldRootContext,
            Pipe<EmitSyntax>    ldLookback,
            Type                rootContextType,
            LocalParseContext[] localContexts = null)
        {
            this.emit = emit;
            this.ldRootContext   = ldRootContext;
            this.ldLookback      = ldLookback;
            this.RootContextType = rootContextType;
            this.localContexts   = localContexts;
        }

        public Type RootContextType { get; set; }

        public void LdContextType(Type type)
        {
            if (localContexts != null && localContexts.Length != 0)
            {
                var locals =
                    (from lc in localContexts
                     where type.IsAssignableFrom(lc.ChildType)
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
                         select new IntArrow<int>(lc.ParentState, i))
                        .ToArray();

                    var switchGenerator = SwitchGenerator.Sparse(
                                            map,
                                            labels.Length - 1,
                                            new IntInterval(0, short.MaxValue));
                    System.Linq.Expressions.Expression<Action<IStackLookback<Msg>>> lookback = lb => lb.GetParentState();
                    switchGenerator.Build(
                        emit,
                        new Pipe<EmitSyntax>(il => il.Do(ldLookback).Call<IStackLookback<Msg>>(lookback)),
                        (il, value) =>
                        {
                            il.Label(labels[value].Def);

                            if (value == locals.Length)
                            {
                                if (LdGlobalContext(type))
                                {
                                    il.Br(END);
                                }
                                else
                                {
                                    il
                                        .Ldstr(new QStr("Internal error: missing global context : " + type.FullName))
                                        .Newobj((string msg) => new Exception(msg))
                                        .Throw()
                                        ;
                                }
                            }
                            else
                            {
                                var lc = locals[value];
                                if (LdRelativeContext(
                                        lc.ContextTokenType,
                                        il2 => il2
                                            // Lookback for getting parent instance
                                                .Do(ldLookback)
                                                .Ldc_I4(lc.ContextLookbackPos)
                                                .Call((IStackLookback<Msg> lb, int backOffset) 
                                                        => lb.GetValueAt(backOffset))
                                                .Ldfld((Msg msg) => msg.Value),
                                        type))
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
            
            if (!LdGlobalContext(type))
            {
                throw new InvalidOperationException(
                    "Context type '" + type.FullName + "' is not accessible.");
            }
        }

        private bool LdGlobalContext(Type type)
        {
            return LdRelativeContext(RootContextType, ldRootContext, type); 
        }

        private bool LdRelativeContext(Type fromContext, Pipe<EmitSyntax> ldFromContext, Type type)
        {
            var contextBrowser = new ContextBrowser(fromContext);

            var path = contextBrowser.GetGetterPath(type);
            if (path == null)
            {
                return false;
            }

            // Load root context
            emit.Do(ldFromContext);

            foreach (var getter in path)
            {
                emit.Call(getter);
            }

            return true;
        }
    }
}
