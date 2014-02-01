using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Logging;
using IronText.Reflection;
using IronText.Reflection.Managed;
using IronText.Runtime;

namespace IronText.MetadataCompiler
{
    /// <summary>
    /// Bootstrapping lexer based on the Regex class.
    /// </summary>
    class BootstrapScanner
        : IScanner
        , IEnumerable<Msg>
    {
        private delegate object TokenFactoryDelegate(string text, object rootContext);

        private readonly Regex regex;
        private readonly string text;
        private ScannerDescriptor descriptor;
        private TokenFactoryDelegate[] tokenFactories;
        private readonly object rootContext;
        private readonly ILogging logging;

        public BootstrapScanner(
                string input,
                ScannerDescriptor descriptor,
                object rootContext,
                ILogging logging)
            : this(new StringReader(input), Loc.MemoryString, descriptor, rootContext, logging)
        { }

        public BootstrapScanner(
                TextReader        textSource,
                string            document,
                ScannerDescriptor descriptor,
                object            rootContext,
                ILogging          logging)
        {
            this.descriptor = descriptor;
            this.rootContext = rootContext;
            this.logging = logging;

            var pattern = @"\G(?:" + string.Join("|", descriptor.Productions.Select(scanProd => "(" + GetPattern(scanProd) + ")")) + ")";
            this.regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);
            this.text = textSource.ReadToEnd();

            int count = descriptor.Productions.Count;
            this.tokenFactories = new TokenFactoryDelegate[count];

            for (int i = 0; i != count; ++i)
            {
                if (descriptor.Productions[i].Outcome != null)
                {
                    tokenFactories[i] = BuildTokenFactory(descriptor.Productions[i]);
                }
            }
        }

        private static string GetPattern(Matcher production)
        {
            return production.Pattern.BootstrapPattern;
        }

        public IReceiver<Msg> Accept(IReceiver<Msg> visitor)
        {
            return visitor.Feed(Tokenize()).Done();
        }

        private IEnumerable<Msg> Tokenize()
        {
            int currentPos = 0;

            if (text.Length != 0)
            {
                var match = this.regex.Match(this.text);
                for (; match.Success; match = match.NextMatch())
                {
                    currentPos = match.Index + match.Length;

                    int termIndex = Enumerable
                                    .Range(1, match.Groups.Count)
                                    .First(i => match.Groups[i].Success) - 1;
                    var production = descriptor.Productions[termIndex];
                    if (production.Outcome == null)
                    {
                        continue;
                    }

                    object termValue = tokenFactories[termIndex](match.Value, this.rootContext);
                    int    tokenId = production.Outcome.Index;

                    yield return
                        new Msg(
                            tokenId,
                            termValue,
                            new Loc(Loc.MemoryString, match.Index, match.Length));

                    if (currentPos == text.Length)
                    {
                        break;
                    }
                }

                if (!match.Success)
                {
                    logging.Write(
                        new LogEntry
                        {
                            Severity = Severity.Error,
                            Message = "Unexpected char: '" + text[currentPos] + "' at " + (currentPos + 1)
                        });
                }
            }
        }

        private static TokenFactoryDelegate BuildTokenFactory(Matcher scanProduction)
        {
            var method = new DynamicMethod(
                                "Create",
                                typeof(object), 
                                new[] { typeof(string), typeof(object) },
                                typeof(BootstrapScanner).Module);

            var il = method.GetILGenerator(256);

            if (scanProduction.Pattern.IsLiteral)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                var binding = scanProduction.Joint.The<CilMatcher>();
                if (binding == null)
                {
                    throw new InvalidOperationException("ScanProduction is missing CIL platform binding.");
                }

                Type type = binding.BootstrapResultType;

                MethodInfo parseMethod = GetStaticParseMethod(type);
                if (parseMethod != null)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Call, parseMethod);
                }
                else if (type == typeof(string))
                {
                    il.Emit(OpCodes.Ldarg_0);
                }
                else
                {
                    ConstructorInfo constructor = null;
                    Type[] paramTypes = SelectConstructor(type, ref constructor);
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
            }

            il.Emit(OpCodes.Ret);
            return (TokenFactoryDelegate)method.CreateDelegate(typeof(TokenFactoryDelegate));
        }

        private static Type[] SelectConstructor(Type type, ref ConstructorInfo constructor)
        {
            Type[][] signatures = {
                    new Type[] { typeof(string) },
                    Type.EmptyTypes
                };


            Type[] paramTypes = null;
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
            return paramTypes;
        }

        public IEnumerator<Msg> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private static MethodInfo GetStaticParseMethod(Type type)
        {
            if (type == null)
            {
                return null;
            }

            var m = type.GetMethod(
                        "BootstrapParse",
                        BindingFlags.Static | BindingFlags.Public,
                        null, new[] { typeof(string) },
                        null)
                     ??
                     type.GetMethod(
                        "Parse",
                        BindingFlags.Static | BindingFlags.Public,
                        null, new[] { typeof(string) },
                        null)
                     ;
            return m;
        }
    }
}
