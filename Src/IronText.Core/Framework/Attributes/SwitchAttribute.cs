#if SWITCH_FEATURE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Framework;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SwitchAttribute : LanguageMetadataAttribute
    {
        public override void Bind(ILanguageMetadata parent, MemberInfo member)
        {
            var tokenType = ((MethodInfo)member).ReturnType;
            if (!typeof(IReceiver<Msg>).IsAssignableFrom(tokenType))
            {
                throw new InvalidOperationException("Switch token should implement IReciever<Msg> type.");
            }

            base.Bind(parent, member);
        }

        public override IEnumerable<SwitchRule> GetSwitchRules(IEnumerable<TokenRef> tokens, ITokenPool tokenPool)
        {
            var tokenType = ((MethodInfo)Member).ReturnType;
            var tid = tokens.FirstOrDefault(t => t.TokenType == tokenType);
            if (tid == null)
            {
                return base.GetSwitchRules(new [] { tid }, tokenPool);
            }

            return GetSwitchRules(tid, tokenPool);
        }

        private IEnumerable<SwitchRule> GetSwitchRules(TokenRef token, ITokenPool tokenPool)
        {
            return new [] {
                new SwitchRule
                {
                    Tid = token,
                    ActionBuilder = code =>
                        {
                            var method = (MethodInfo)Member;

                            if (!method.IsStatic)
                            {
                                code.ContextResolver.LdContextType(method.DeclaringType);
                            }

                            var parameters = method.GetParameters();
                            foreach (var param in parameters)
                            {
                                if (param.ParameterType == typeof(ILanguage))
                                {
                                    code.LdLanguage();
                                }
                                else if (param.ParameterType == typeof(IReceiver<Msg>))
                                {
                                    code.LdExitReceiver();
                                }
                                else
                                {
                                    throw new InvalidOperationException(
                                        "Unsupported switch-method signature: " 
                                        + string.Join(", ", parameters.Select(p => p.ParameterType.Name)));
                                }
                            }

                            code.Emit(il => il.Call(method));

                            if (method.ReturnType.IsValueType)
                            {
                                code.Emit(il => il.Box(il.Types.Import(method.ReturnType)));
                            }

                            code.Emit(il => il.Ret());
                        }
                }
            };
        } 
    }
}

#endif
