using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Reflection.Managed;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    public class MergeAttribute : LanguageMetadataAttribute
    {
        private MethodInfo Method { get { return (MethodInfo)Member; } }

        public override IEnumerable<CilMerger> GetMergers(IEnumerable<CilSymbolRef> leftSides)
        {
            var returnToken = CilSymbolRef.Create(Method.ReturnType);
            if (!leftSides.Contains(returnToken))
            {
                return new CilMerger[0];
            }

            var type = Method.ReturnType;
            var method = Method;

            return new []
            { 
                new CilMerger
                {
                    Symbol  = returnToken,
                    Context = GetContext(),
                    ActionBuilder = code =>
                        {
                            code = code.LdMergerOldValue();
                            if (type.IsValueType)
                            {
                                code = code.Emit(il => il.Unbox_Any(il.Types.Import(type)));
                            }
                                
                            code = code.LdMergerNewValue();
                            if (type.IsValueType)
                            {
                                code = code.Emit(il => il.Unbox_Any(il.Types.Import(type)));
                            }

                            code = code
                                .Emit(il => il.Call(il.Methods.Import(method)));

                            if (type.IsValueType)
                            {
                                code = code.Emit(il => il.Box(il.Types.Import(type)));
                            }

                            return code;
                        }
                }
            };
        }

        private CilContextRef GetContext()
        {
            var contextType = GetContextType();
            var contextRef = Method.IsStatic ? CilContextRef.None : CilContextRef.ByType(contextType);
            return contextRef;
        }
    }
}
