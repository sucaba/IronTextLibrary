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
        public int Precedence { get; set; }

        public Associativity Assoc { get; set; }

        protected virtual MethodInfo Method
        {
            get { return (MethodInfo)Member; }
        }

        private Precedence GetPrecedence()
        {
            if (Precedence != 0 || Assoc != Associativity.None)
            {
                return new Precedence(Precedence, Assoc);
            }

            return null;
        }

        protected abstract TokenRef[] DoGetRuleMask(MethodInfo methodInfo, ITokenPool tokenPool);

        public override IEnumerable<ParseRule> GetParseRules(IEnumerable<TokenRef> tokens, ITokenPool tokenPool)
        {
            return tokens.SelectMany(token => GetExpansionRules(token, tokenPool)).ToArray();
        }

        private IEnumerable<ParseRule> GetExpansionRules(TokenRef token, ITokenPool tokenPool)
        {
            if (token.IsLiteral)
            {
                return Enumerable.Empty<ParseRule>();
            }

            var method = this.Method;
            if (MethodDefinesRuleTemplates(method, token.TokenType))
            {
                MethodInfo instantiatedMethod = MakeGenericRuleMethod(token.TokenType, method);
                return DoGetRules(tokenPool, instantiatedMethod, token);
            }
            else
            {
                return DoGetRules(tokenPool, method, token);
            }
        }

        private bool MethodDefinesRuleTemplates(MethodInfo method, Type leftSideTokenType)
        {
            if (!method.IsGenericMethodDefinition)
            {
                // Only generic methods are considered as rule templates
                return false;
            }

            var returnType = method.ReturnType;
            if (returnType.IsArray)
            {
                return leftSideTokenType.IsArray
                    && returnType.GetElementType().IsGenericParameter;
            }

            return returnType.IsGenericType
                && leftSideTokenType.IsGenericType
                && returnType.GetGenericTypeDefinition() == leftSideTokenType.GetGenericTypeDefinition();
        }

        private static MethodInfo MakeGenericRuleMethod(Type tokenType, MethodInfo method)
        {
            Type[] parameters;
            if (method.ReturnType.IsArray)
            {
                parameters = new[] { tokenType.GetElementType() };
            }
            else
            {
                parameters = tokenType.GetGenericArguments();
            }

            return method.MakeGenericMethod(parameters);
        }

        public override IEnumerable<Type> GetContextTypes()
        {
            yield break;
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
            {
                Left          = left,
                Parts         = parts.ToArray(),
                InstanceDeclaringType = method.IsStatic ? null : method.DeclaringType,
                IsContextRule = thisToken != null,
                Precedence    = GetPrecedence(),
                ActionBuilder =
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
            };

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
    }
}
