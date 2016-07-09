﻿using System.Linq;
using IronText.Extensibility;
using IronText.Framework;
using IronText.Reflection;
using IronText.Lib.Ctem;
using IronText.MetadataCompiler;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using IronText.Logging;
using IronText.Runtime;

namespace IronText.Tests.Algorithm
{
    [TestFixture]
    public class DfaSimulationLexerTest
    {
        [Test]
        public void Test()
        {
            var lParen = new Symbol("(");
            var rParen = new Symbol(")");
            var num    = new Symbol("NUM");
            var ident  = new Symbol("ID");
            var qStr   = new Symbol("QSTR");

            var grammar = new Grammar
            {
                Symbols = 
                { 
                    lParen,
                    rParen,
                    num,
                    ident,
                    qStr
                },

                Matchers = 
                {
                    new Matcher(@"blank+"),
                    new Matcher(@"digit+ ('.' digit+)?  | '.' digit+", num),
                    new Matcher(
                            @"(alpha | [:.!@#$%^&|?*/+*=\\_-]) (alnum | [:.!@#$%^&|?*/+*=\\_-])*",
                            ident),
                    new Matcher("'\"' ('\\\\\"' | ~'\"')* '\"'", qStr),
                    new Matcher(ScanPattern.CreateLiteral("("), lParen),
                    new Matcher(ScanPattern.CreateLiteral(")"), rParen),
                }
            };

            grammar.BuildIndexes();
           
            var target = new DfaSimulationLexer(
                " (1 (\"bar\" +))",
                ScannerDescriptor.FromScanRules(grammar.Matchers, ExceptionLogging.Instance));

            var collector = new Collector<Message>();
            target.Accept(collector);
            Assert.AreEqual(
                new int[] { lParen.Index, num.Index, lParen.Index, qStr.Index, ident.Index, rParen.Index, rParen.Index },
                collector.Select(msg => msg.AmbiguousToken).ToArray());
            Assert.AreEqual(
                new object[] { "(", "1", "(", "\"bar\"", "+", ")", ")" },
                collector.Select(msg => msg.Text).ToArray());
        }
    }
}
