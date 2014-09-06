using IronText.Framework;
using IronText.Reflection;
using IronText.Runtime;
using IronText.Tests.TestUtils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Semantics
{
    [TestFixture]
    public class EclrAttributesTest
    {
        [Test]
        public void Test()
        {
            var grammar = new Grammar
            {
                StartName = "S",
                Productions =
                {
                    { "S",       new [] { "DclList", "StList" } },
                    { "DclList", new [] { "DcList", "Dc" } },
                    { "DclList", new [] { "Dc" } },
                    { "StList",  new [] { "StList", "St" } },
                    { "StList",  new [] { "St" } },
                    { "Dc",      new [] { "'dcl'", "id" } },
                    { "St",      new [] { "'use'", "id" } },
                },
                Matchers = 
                {
                    { "dcl" },
                    { "use" },
                    { "id", "(alpha | '_') (alnum | '_')*" },
                    { null, "blank+" }
                }
            };

            var sut = new ParserSut(grammar);
            sut.Parse("dcl x dcl y use x use y");
        }

#if false
        [Language]
        [GrammarDocument("DeclareUseLang.gram")]
        public interface DeclareUseLang
        {
            [Produce]
            void All(DcList dclist, StList stlist);

            [Produce]
            DcList DcList(Dc item);

            [Produce]
            StList StList(StList dclist, St item, [Semantic] Env env);

            [Produce]
            StList StList(St item);

            [Produce("dcl")]
            Dc Declare(string id);

            [Produce("use")]
            St Use(string id, [Semantic] Env env);

            [Match("'a'..'z'")]
            string Identifier();

            [Match("blank+")]
            void Blank();
        }

        public interface Env { }

        [Demand]
        public interface DcList 
        { 
            [SubContext]
            Env Env { get; }

            [Produce]
            DcList DcList(Dc item);
        }

        public interface StList { }
        public interface Dc { }
        public interface St { }
#endif
    }
}
