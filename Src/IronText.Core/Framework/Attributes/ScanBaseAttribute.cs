using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Lib.Shared;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Logging;
using IronText.Misc;

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

        public override bool Validate(ILogging logging)
        {
            var parameters = Method.GetParameters().ToList();
            var signature = GetSignatureKind(parameters);
            if (signature == SignatureKind.Invalid)
            {
                string message =
                    string.Format(
                        "Unsupported match-method signature in {0}::{1}",
                        Member.DeclaringType.Name,
                        Member,
                        string.Join(", ", parameters.Select(p => p.ParameterType.Name)));

                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Message = message,
                        Origin = ReflectionUtils.ToString(Member)
                    });
            }

            return base.Validate(logging);
        }

        public override IEnumerable<CilMatcher> GetMatchers()
        {
            var tokenType    = Method.ReturnType;

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
            var signature = GetSignatureKind(parameters);
            if (signature == SignatureKind.Invalid)
            {
                return base.GetMatchers();
            }

            matcher.Context = GetContext();
            matcher.ActionBuilder =
                code =>
                {
                    switch (signature)
                    {
                        case SignatureKind.NoArgs:
                            break;
                        case SignatureKind.StringArg: 
                            code.LdMatcherTokenString();
                            break;
                    }

                    code.Emit(il => il.Call(Method));

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

                        code.ReturnFromAction() ;
                    }

                    return code;
                };


            return new[] { matcher };
        }

        private CilContextRef GetContext()
        {
            return Method.IsStatic ? CilContextRef.None : CilContextRef.ByType(GetContextType());
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

        private static SignatureKind GetSignatureKind(IList<ParameterInfo> parameters)
        {
            SignatureKind result;
            if (parameters.Count == 0)
            {
                result = SignatureKind.NoArgs;
            }
            else if (parameters.Count == 1
                    && parameters[0].ParameterType == typeof(string))
            {
                result = SignatureKind.StringArg;
            }
            else
            {
                result = SignatureKind.Invalid;
            }

            return result;
        }

        enum SignatureKind
        {
            Invalid,
            NoArgs,
            StringArg
        }
    }
}
