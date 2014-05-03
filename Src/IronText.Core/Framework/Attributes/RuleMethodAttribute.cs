using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Logging;
using IronText.Misc;
using IronText.Reflection;
using IronText.Reflection.Managed;

namespace IronText.Framework
{
    public abstract class RuleMethodAttribute : LanguageMetadataAttribute
    {
        private bool isValid;

        public int Precedence { get; set; }

        public Associativity Associativity { get; set; }

        protected virtual MethodInfo Method
        {
            get { return (MethodInfo)Member; }
        }

        private Precedence GetPrecedence()
        {
            if (Precedence != 0 || Associativity != Associativity.None)
            {
                return new Precedence(Precedence, Associativity);
            }

            return null;
        }

        public override bool Validate(ILogging logging)
        {
            isValid = 
                ValidateAllGenericArgsAreUsed(Method.GetGenericArguments().ToList(), Method.ReturnType, logging)
                &&
                base.Validate(logging);

            return isValid;
        }

        protected abstract CilSymbolRef[] DoGetRuleMask(MethodInfo methodInfo);

        public override IEnumerable<CilProduction> GetProductions(IEnumerable<CilSymbolRef> tokens)
        {
            if (!isValid)
            {
                return Enumerable.Empty<CilProduction>();
            }

            return tokens.SelectMany(token => GetExpansionRules(token)).ToArray();
        }

        private IEnumerable<CilProduction> GetExpansionRules(CilSymbolRef token)
        {
            if (token.HasLiteral)
            {
                return Enumerable.Empty<CilProduction>();
            }

            var method = this.Method;
            var pattern = new TypePattern(method);
            MethodInfo producer = pattern.MakeProducer(token.Type);
            if (producer != null)
            {
                return DoGetRules(producer, token);
            }
            else
            {
                return Enumerable.Empty<CilProduction>();
            }
        }

        protected CilSymbolRef GetThisSymbol()
        {
            if (HasThisAsSymbol)
            {
                if (Parent != null && Parent.Member is Type)
                {
                    return CilSymbolRef.Create((Type)Parent.Member);
                }

                return CilSymbolRef.Create(Member.DeclaringType);
            }

            return null;
        }

        protected bool DetectThisAsToken(List<CilSymbolRef> parts)
        {
            if (HasThisAsSymbol)
            {
                parts.Add(CilSymbolRef.Create(Member.DeclaringType));
                return true;
            }

            return false;
        }

        private bool HasThisAsSymbol
        {
            get
            {
                var demandAttr = Attributes.First<DemandAttribute>(Member.DeclaringType);
                return demandAttr != null && demandAttr.Value;
            }
        }

        protected IEnumerable<CilProduction> DoGetRules(MethodInfo method, CilSymbolRef leftSide)
        {
            var outcome = CilSymbolRef.Create(method.ReturnType);

            if (!object.Equals(outcome, leftSide))
            {
                return Enumerable.Empty<CilProduction>();
            }

            var pattern = new List<CilSymbolRef>();
            int argShift = 0;

            CilSymbolRef thisSymbol = GetThisSymbol();
            if (thisSymbol != null)
            {
                ++argShift;
                pattern.Add(thisSymbol);
            }

            var ruleMask = DoGetRuleMask(method);

            SubstituteRuleMask(method, pattern, ruleMask);

            var rule = new CilProduction(
                outcome:    outcome,
                pattern:    pattern,
                precedence: GetPrecedence(),
                context:    GetContext(method, thisSymbol != null),
                actionBuilder:
                    code =>
                    {
                        if (thisSymbol != null)
                        {
                            code.LdActionArgument(0, method.DeclaringType);
                        }

                        // Pass rule-arguments to the rule-method
                        for (int i = 0; i != method.GetParameters().Length; ++i)
                        {
                            var param = method.GetParameters()[i];
                            int ruleArgIndex = NthEmptySlotIndex(ruleMask, i);
                            if (thisSymbol != null)
                            {
                                ++ruleArgIndex;
                            }

                            code.LdActionArgument(ruleArgIndex, param.ParameterType);
                        }

                        code.Emit(
                            il =>
                            {
                                il.Call(method);

                                if (method.ReturnType == typeof(void))
                                {
                                    il.Ldnull();
                                }
                                else if (method.ReturnType.IsValueType)
                                {
                                    il.Box(il.Types.Import(method.ReturnType));
                                }

                                return il;
                            });

                        return code;
                    });

            return new[] { rule };
        }

        private CilSemanticRef GetContext(MethodInfo method, bool hasThis)
        {
            if (method.IsStatic || hasThis)
            {
                return CilSemanticRef.None;
            }

            var contextType = GetContextType();

            // Local or global context identified by type
            return CilSemanticRef.ByType(contextType);
        }

        private static int NthEmptySlotIndex(CilSymbolRef[] ruleMask, int n)
        {
            int i = 0;
            for (; i != ruleMask.Length; ++i)
            {
                if (ruleMask[i] == null)
                {
                    if (n == 0)
                    {
                        return i;
                    }

                    --n;
                }
            }

            return i + n;
        }

        private static void SubstituteRuleMask(MethodInfo method, List<CilSymbolRef> parts, CilSymbolRef[] ruleMask)
        {
            int maskLength = ruleMask.Length;
            int paramCount = method.GetParameters().Length;

            for (int i = 0, paramIndex = 0; i != maskLength || paramIndex != paramCount; )
            {
                if (paramIndex == paramCount)
                {
                    // Mask has keywords after parameters
                    if (ruleMask[i] == null)
                    {
                        throw new InvalidOperationException("Insufficient parameters to substitute literal-mask slot #" + i);
                    }

                    parts.Add(ruleMask[i]);
                }
                else if (i == maskLength || ruleMask[i] == null)
                {
                    // Mask slot or there are parameters after mask
                    var p = method.GetParameters()[paramIndex++];
                    parts.Add(CilSymbolRef.Create(p.ParameterType));
                }
                else
                {
                    // Add keyword from the rule mask
                    parts.Add(ruleMask[i]);
                }

                if (i != maskLength)
                {
                    ++i;
                }
            }
        }

        private bool ValidateAllGenericArgsAreUsed(
            List<Type> usedTypes,
            Type       compositeType,
            ILogging   logging)
        {
            DeleteUsedGenericArgs(usedTypes, compositeType, logging);
            if (usedTypes.Count != 0)
            {
                logging.Write(
                    new LogEntry 
                    {
                        Severity = Severity.Error,
                        Message  = "One or more generic arguments cannot be deduced from the method result type.",
                        Origin   = ReflectionUtils.ToString(Method)
                    });
                return false;
            }

            return true;
        }

        private static void DeleteUsedGenericArgs(
            List<Type> usedTypes,
            Type type,
            ILogging logging)
        {
            if (type.IsGenericParameter)
            {
                usedTypes.Remove(type);
            }
            else if (type.ContainsGenericParameters)
            {
                if (type.IsArray)
                {
                    DeleteUsedGenericArgs(usedTypes, type.GetElementType(), logging);
                }
                else
                {
                    foreach (var paramType in type.GetGenericArguments())
                    {
                        DeleteUsedGenericArgs(usedTypes, paramType, logging);
                    }
                }
            }
        }
    }
}
