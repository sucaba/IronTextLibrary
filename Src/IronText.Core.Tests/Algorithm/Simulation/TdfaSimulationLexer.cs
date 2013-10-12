using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using IronText.Automata.Regular;
using IronText.Diagnostics;
using IronText.Extensibility;
using IronText.Framework;
using IronText.MetadataCompiler;

namespace IronText.Tests.Algorithm
{
    /// <summary>
    /// Lexer class to test and debug DFA semantics.
    /// </summary>
    public class TdfaSimulationLexer : ISequence<Msg>
    {
        private delegate object TokenFactoryDelegate(string text);

        private ITdfaSimulation tdfa;
        private readonly string text;
        private ScannerDescriptor descriptor;
        // rule index -> token factory mapping:
        private TokenFactoryDelegate[] tokenFactories;

        public TdfaSimulationLexer(string text, ScannerDescriptor descriptor)
            : this(new StringReader(text), descriptor)
        { }

        public TdfaSimulationLexer(TextReader textSource, ScannerDescriptor descriptor)
        {
            this.text = textSource.ReadToEnd();
            this.descriptor = descriptor;

            Dictionary<string, int> literalToAction = new Dictionary<string,int>();
            var root = descriptor.MakeAst(literalToAction);
            var regTree = new RegularTree(root);
            var algorithm = new RegularToTdfaAlgorithm(regTree, literalToAction);
            DescribeTdfa(algorithm.Data);
            this.tdfa = new TdfaSimulation(algorithm.Data);

            int count = descriptor.Rules.Count;
            this.tokenFactories = new TokenFactoryDelegate[count];

            for (int i = 0; i != count; ++i)
            {
                if (!(descriptor.Rules[i] is ISkipScanRule))
                {
                    tokenFactories[i] = BuildTokenFactory((ISingleTokenScanRule)descriptor.Rules[i]);
                }
            }
        }

        private void DescribeTdfa(ITdfaData data)
        {
            using (IGraphView view = new GvGraphView("tdfa.gv"))
            {
                data.DescribeGraph(view);
            }
        }

        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            int start = 0, pos = 0;

            int length = text.Length;

            while (pos != length)
            {
                int state = tdfa.Start;
                int? acceptingState = null;

                // Find longest token
                for (; pos != length; )
                {
                    int item = text[pos];

                    int next;
                    if (tdfa.TryNext(state, item, out next))
                    {
                        state = next;
                        if (tdfa.IsAccepting(state))
                        {
                            acceptingState = state;
                        }

                        ++pos;
                    }
                    else if (tdfa.Tunnel(state, out next))
                    {
                        state = next;
                    }
                    else
                    {
                        break;
                    }
                }

                // Fail
                if (!acceptingState.HasValue)
                {
                    throw new Exception("Unexpected character at pos " + pos);
                }

                int ruleIndex = tdfa.GetAction(acceptingState.Value) ?? -1;
                int tokenLength = pos - start;

                var rule = descriptor.Rules[ruleIndex];

                if (!(rule is ISkipScanRule))
                {
                    TokenFactoryDelegate tokenFactory = tokenFactories[ruleIndex];

                    if (tokenFactory != null)
                    {
                        // Emit next token
                        visitor = visitor.Next(
                            new Msg(
                                -1,
                                tokenFactory(text.Substring(start, tokenLength)),
                                new Loc(Loc.MemoryString, start, pos)));
                    }
                }

                start = pos;
            }

            return visitor.Done();
        }

        private static TokenFactoryDelegate BuildTokenFactory(ISingleTokenScanRule scanRule)
        {
            Type type = scanRule.TokenType;

            if (type == typeof(void))
            {
                return null;
            }

            if (type != null)
            {
                MethodInfo parseMethod = GetStaticParseMethod(type);
                if (parseMethod != null)
                {
                    return (TokenFactoryDelegate)Delegate.CreateDelegate(
                        typeof(TokenFactoryDelegate),
                        parseMethod);
                }
            }
            
            var method = new DynamicMethod("Create", typeof(object), new[] { typeof(string) }, typeof(DfaSimulationLexer).Module);
            var il = method.GetILGenerator(256);

            if (type == null || scanRule.LiteralText != null)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            else
            {
                Type[][] signatures = {
                    new Type[] { typeof(string) },
                    Type.EmptyTypes
                };

                Type[] paramTypes = null;
                ConstructorInfo constructor = null;
                foreach (var signature in signatures)
                {
                    constructor = type.GetConstructor(signature);
                    if (constructor != null)
                    {
                        paramTypes = signature;
                        break;
                    }
                }

                if (constructor == null)
                {
                    throw new InvalidOperationException(
                        "Type " + type.FullName + " has no public constructor suitable for a lexer.");
                }

                if (paramTypes.Length > 0)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    if (paramTypes.Length > 1)
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                }

                il.Emit(OpCodes.Newobj, constructor);
            }

            il.Emit(OpCodes.Ret);
            return (TokenFactoryDelegate)method.CreateDelegate(typeof(TokenFactoryDelegate));
        }

        private static MethodInfo GetStaticParseMethod(Type type)
        {
            var m = type.GetMethod(
                        "Parse",
                        BindingFlags.Static | BindingFlags.Public,
                        null, new[] { typeof(string) },
                        null);
            return m;
        }
    }
}
