using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework
{
    /// <summary>
    /// Use-case and sample for local flow of syntax
    /// objects in programmatic scenario and compatibility
    /// with text parsing scenario.
    /// </summary>
    [TestFixture]
    public class LocalSyntaxPipingTest
    {
        [Test]
        public void ProgrammaticTest()
        {
            SyntaxPipingLang context = new SyntaxPipingLangImpl();
            context.Result 
                = context.Method(_=>_ .StartMethod .Return("int") .Name("foo"))
                ;

            var result = context.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("int", result.ResultType);
            Assert.AreEqual("foo", result.Name);

            // Without optional return type
            context = new SyntaxPipingLangImpl();
            context.Result 
                = context.Method(_=>_ .StartMethod.Name("foo"))
                ;

            result = context.Result;
            Assert.IsNotNull(result);
            Assert.IsNull(result.ResultType);
            Assert.AreEqual("foo", result.Name);

            // Without any information (no return type and name)
            context = new SyntaxPipingLangImpl();
            context.Result 
                = context.Method(_=> (DoneMethod)context)
                ;

            result = context.Result;
            Assert.IsNotNull(result);
            Assert.IsNull(result.ResultType);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void ParsingTest()
        {
            SyntaxPipingLang context = new SyntaxPipingLangImpl();
            Language.Parse(context, "method int foo ()");

            var result = context.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual("int", result.ResultType);
            Assert.AreEqual("foo", result.Name);

            // without return type
            context = new SyntaxPipingLangImpl();
            Language.Parse(context, "method foo ()");

            result = context.Result;
            Assert.IsNotNull(result);
            Assert.IsNull(result.ResultType);
            Assert.AreEqual("foo", result.Name);

            // without return type and name
            context = new SyntaxPipingLangImpl();
            Language.Parse(context, "method");

            result = context.Result;
            Assert.IsNotNull(result);
            Assert.IsNull(result.ResultType);
            Assert.IsNull(result.Name);
        }

        [Language]
        [GrammarDocument("SyntaxPiping.gram")]
        [ScannerDocument("SyntaxPiping.scan")]
        [DescribeParserStateMachine("SyntaxPiping.info")]
        [StaticContext(typeof(Utils))]
        public interface SyntaxPipingLang
        {
            [ParseResult]
            MethodSig Result { get; set; }

            // Instead of C parameter there is Pipe<A,C> wich is 
            // constructed from C in parse scenario (via generic 
            // rule method) i.e. is not really useful.
            // However in programmatic scenario this pipe serves
            // as additional indirection level which can be used
            // for more convenient programmatic code generation
            // (see test code for more details).
            [Parse("method")]
            MethodSig Method(Pipe<SyntaxPipingLang,DoneMethod> code);

            [Parse("call")]
            MethodCall Call(Pipe<SyntaxPipingLang,DoneMethod> code);

            [ParseGet]
            WantReturn StartMethod { get; }
        }

        [Vocabulary]
        public static class Utils
        {
            [Parse]
            public static Pipe<S,R> Pipe<S,R>(R ret) { return start => ret; }

            [Scan("alpha alnum+")]
            public static string Identifier(string id) { return id; }

            [Scan("blank+")]
            public static void Blank() { }
        }

        #region Syntaxes defining order

        [Demand]
        public interface WantReturn 
            : WantReturnThen<WantName>
            , WantName 
        { }

        [Demand]
        public interface WantName
            : WantNameThen<DoneMethod>
            , EmptyThen<DoneMethod>
        { }

        public interface DoneMethod { }

        #endregion

        #region Syntaxes defining structure

        [Demand]
        public interface WantReturnThen<TNext>
        {
            [Parse]
            TNext Return(string value);
        }

        [Demand]
        public interface WantNameThen<TNext>
        {
            [Parse(null, "(", ")")]
            TNext Name(string value);
        }

        [Demand]
        public interface EmptyThen<TNext>
        {
            [Parse]
            TNext End();
        }

        #endregion

        public class MethodSig
        {
            public MethodSig() { }

            public string ResultType { get; set; }
            public string Name { get; set; }
        }

        public class MethodCall
        {
            public MethodSig MethodSig { get; set; }
        }

        public class SyntaxPipingLangImpl 
            : SyntaxPipingLang
            , WantReturn
            , WantName
            , DoneMethod
        {
            private MethodSig currentMethodSig = new MethodSig();

            MethodSig SyntaxPipingLang.Result { get; set; }

            MethodSig SyntaxPipingLang.Method(Pipe<SyntaxPipingLang,DoneMethod> code)
            {
                code(this);
                return currentMethodSig;
            }

            MethodCall SyntaxPipingLang.Call(Pipe<SyntaxPipingLang,DoneMethod> code)
            {
                return new MethodCall { MethodSig = currentMethodSig };
            }

            WantReturn SyntaxPipingLang.StartMethod { get { return this; } }

            WantName WantReturnThen<WantName>.Return(string value) 
            {
                currentMethodSig.ResultType = value;
                return this;
            }

            DoneMethod WantNameThen<DoneMethod>.Name(string value) 
            {
                currentMethodSig.Name = value;
                return this; 
            }

            DoneMethod EmptyThen<DoneMethod>.End()
            {
                return this;
            }
        }
    }
}
