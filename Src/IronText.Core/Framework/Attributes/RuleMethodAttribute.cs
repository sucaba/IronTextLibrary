using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IronText.Extensibility;
using IronText.Lib.IL;
using IronText.Misc;

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

        protected abstract TokenRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool);

        public override IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> tokens, ITokenPool tokenPool)
        {
            if (!isValid)
            {
                return Enumerable.Empty<ParseRule>();
            }

            return tokens.SelectMany(token => GetExpansionRules(token, tokenPool)).ToArray();
        }

        private IEnumerable<ParseRule> GetExpansionRules(TokenRef token, ITokenPool tokenPool)
        {
            if (token.IsLiteral)
            {
                return Enumerable.Empty<ParseRule>();
            }

            var method = this.Method;
            var pattern = new TypePattern(method);
            MethodInfo producer = pattern.MakeProducer(token.TokenType);
            if (producer != null)
            {
                return DoGetRules(tokenPool, producer, token);
            }
            else
            {
                return Enumerable.Empty<ParseRule>();
            }
        }

        protected TokenRef GetThisToken(ITokenPool tokenPool)
        {
            if (HasThisAsToken)
            {
                if (Parent != null && Parent.Member is Type)
                {
                    return tokenPool.GetToken((Type)Parent.Member);
                }

                return tokenPool.GetToken(Member.DeclaringType);
            }

            return null;
        }

        protected bool DetectThisAsToken(ITokenPool tokenPool, List<TokenRef> parts)
        {
            if (HasThisAsToken)
            {
                parts.Add(tokenPool.GetToken(Member.DeclaringType));
                return true;
            }

            return false;
        }

        private bool HasThisAsToken
        {
            get
            {
                var thisAsTokenAttr = Attributes.First<DemandAttribute>(Member.DeclaringType);
                return thisAsTokenAttr != null && thisAsTokenAttr.Value;
            }
        }

        protected IEnumerable<ParseRule> DoGetRules(ITokenPool tokenPool, MethodInfo method, TokenRef leftSide)
        {
            TokenRef left = tokenPool.GetToken(method.ReturnType);

            if (!object.Equals(left, leftSide))
            {
                return Enumerable.Empty<ParseRule>();
            }

            var parts = new List<TokenRef>();
            int argShift = 0;

            TokenRef thisToken = GetThisToken(tokenPool);
            if (thisToken != null)
            {
                ++argShift;
                parts.Add(thisToken);
            }

            var ruleMask = DoGetRuleMask(method, tokenPool);

            SubstituteRuleMask(tokenPool, method, parts, ruleMask);

            var rule = new ParseRule
            (
                left          : left,
                parts         : parts.ToArray(),
                instanceDeclaringType : method.IsStatic ? null : method.DeclaringType,
                isContextRule : thisToken != null,
                precedence    : GetPrecedence(),
                actionBuilder :
                    code =>
                    {
                        // Load main module object (this)
                        if (method.IsStatic)
                        {
                        }
                        else if (thisToken != null)
                        {
                            code.LdRuleArg(0, method.DeclaringType);
                        }
                        else
                        {
                            // Find out what is previous rule from the stack state in stack[stack.length - rule.length - 1]
                            // Previous rule should be from this-token.
                            // Currently loads relative to root, need also relative to the current this-token
                            // Current this-token can be taken from the stack[stack.length - rule.length - prevRule.length].
                            code.ContextResolver.LdContextType(method.DeclaringType);
                        }

                        // Pass rule-arguments to the rule-method
                        for (int i = 0; i != method.GetParameters().Length; ++i)
                        {
                            var param = method.GetParameters()[i];
                            int ruleArgIndex = NthEmptySlotIndex(ruleMask, i);
                            if (thisToken != null)
                            {
                                ++ruleArgIndex;
                            }

                            code.LdRuleArg(ruleArgIndex, param.ParameterType);
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
                    }
            );

            return new[] { rule };
        }

        private static int NthEmptySlotIndex(TokenRef[] ruleMask, int n)
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

        private static void SubstituteRuleMask(ITokenPool tokenPool, MethodInfo method, List<TokenRef> parts, TokenRef[] ruleMask)
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
                        throw new InvalidOperationException("Insufficient parameters to substittute rule-mask slot #" + i);
                    }

                    parts.Add(ruleMask[i]);
                }
                else if (i == maskLength || ruleMask[i] == null)
                {
                    // Mask slot or there are parameters after mask
                    var p = method.GetParameters()[paramIndex++];
                    parts.Add(tokenPool.GetToken(p.ParameterType));
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
                        Message = "One or more generic arguments cannot be deduced from the method result type.",
                        Member = Method
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
