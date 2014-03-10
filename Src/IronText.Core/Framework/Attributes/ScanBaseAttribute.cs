using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;

namespace IronText.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class ScanBaseAttribute : LanguageMetadataAttribute
    {
        protected ScanBaseAttribute() 
        {
            Disambiguation = Disambiguation.Exclusive;
        }

        public Disambiguation Disambiguation { get; protected set; }

        /// <summary>
        /// Raw text
        /// </summary>
        public string LiteralText { get; protected set; }

        /// <summary>
        /// <see cref="ScannerSyntax"/> pattern
        /// </summary>
        public string Pattern { get; protected set; }

        internal string RegexPattern { get; set; }

        private MethodInfo Method { get { return (MethodInfo)Member; } }

        public override IEnumerable<CilMatcher> GetMatchers()
        {
            var tokenType    = Method.ReturnType;
            var nextConditionType = GetNextConditionType();

            var matcher = new CilMatcher();

            var contextType = GetContextType();
            //var context = CilContextRef.ByType(contextType);

            if (tokenType != typeof(void))
            {
                var outcome = CilSymbolRef.Create(tokenType, LiteralText);
                matcher.MainOutcome = outcome;
                matcher.AllOutcomes.Add(outcome);
            }

            matcher.DefiningMethod = Method;
            matcher.Disambiguation = Disambiguation;
            if (LiteralText == null)
            {
                matcher.Pattern = ScanPattern.CreateRegular(Pattern, RegexPattern);
            }
            else
            {
                matcher.Pattern = ScanPattern.CreateLiteral(LiteralText);
            }

            var parameters = Method.GetParameters().ToList();

            matcher.Context = GetContext();
            matcher.NextConditionType = nextConditionType;
            matcher.ActionBuilder =
                code =>
                {
                    ParameterInfo nextModeParameter;
                    if (parameters.Count != 0 && parameters.Last().IsOut)
                    {
                        nextModeParameter = parameters.Last();
                        parameters.RemoveAt(parameters.Count - 1);
                    }
                    else
                    {
                        nextModeParameter = null;
                    }

                    if (parameters.Count == 0)
                    {
                    }
                    else if (parameters.Count == 1
                            && parameters[0].ParameterType == typeof(string))
                    {
                        code.LdMatcherTokenString();
                    }
                    else if (parameters.Count == 3
                            && parameters[0].ParameterType == typeof(char[])
                            && parameters[1].ParameterType == typeof(int)
                            && parameters[2].ParameterType == typeof(int))
                    {
                        code
                            .LdMatcherBuffer()
                            .LdMatcherStartIndex()
                            .LdMatcherLength();
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Unsupported match-method signature: "
                            + string.Join(", ", parameters.Select(p => p.ParameterType.Name)));
                    }

                    Ref<Locals> nextModeVar = null;
                    if (nextModeParameter != null)
                    {
                        code
                            .Emit(il =>
                            {
                                nextModeVar = il.Locals.Generate().GetRef();
                                return il
                                    .Local(nextModeVar.Def, il.Types.Object)
                                    .Ldloca(nextModeVar);
                            });
                    }

                    code.Emit(il => il.Call(Method));

                    if (nextModeParameter != null)
                    {
                        code
                            .Emit(il => il.Ldloc(nextModeVar))
                            .ChangeCondition(nextConditionType);
                    }

                    if (Method.ReturnType == typeof(void))
                    {
                        code.SkipAction();
                    }
                    else
                    {
                        if (Method.ReturnType.IsValueType)
                        {
                            code.Emit(il => il.Box(il.Types.Import(Method.ReturnType)));
                        }

                        code
                            .ReturnFromAction()
                            ;
                    }

                    return code;
                };


            return new[] { matcher };
        }

        private CilContextRef GetContext()
        {
            return Method.IsStatic ? CilContextRef.None : CilContextRef.ByType(GetContextType());
        }

        private Type GetNextConditionType()
        {
            return Method
                    .GetParameters()
                    .Where(p => p.IsOut)
                    .Select(p => p.ParameterType.GetElementType())
                    .SingleOrDefault();
        }

        private Type TokenType 
        { 
            get 
            {
                var result = Method.ReturnType;
                if (result == typeof(object))
                {
                    return null;
                }

                return result;
            }
        }

        private CilSymbolRef[] GetProducedTokens()
        {
            List<CilSymbolRef> resultList = new List<CilSymbolRef>();
            if (LiteralText != null)
            {
                resultList.Add(CilSymbolRef.Create(LiteralText));
            }

            if (TokenType != null)
            {
                resultList.Add(CilSymbolRef.Create(TokenType));
            }

            return resultList.ToArray();
        }
    }
}
