#if false
using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.Semantics
{
    class SemanticAttribute : Attribute
    {

    }

    [TestFixture]
    public class EclrAttributesTest
    {
        [Test]
        public void Test()
        {
            using (var interp = new Interpreter<DeclareUseLang>())
            {
                interp.Parse("dcl x dcl y use x use y");
            }
        }

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
    }
}
#endif
