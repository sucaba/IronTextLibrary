using NUnit.Framework;
using IronText.Framework;
using IronText.Runtime;

namespace IronText.Tests.MetdataCompiler
{
    [TestFixture]
    public class ParserActionEncodingTest
    {
        [Theory]
        public void TestEncodeAndDecode(
            [Values(
                ParserActionKind.Fail,
                ParserActionKind.Accept,
                ParserActionKind.Reduce,
                ParserActionKind.Shift
                )] 
                ParserActionKind kind,
            [Values(0, 1, 1024, ushort.MaxValue)] 
                    int value1)
        {
            int cell = ParserAction.Encode(kind, value1);

            var action = ParserAction.Decode(cell);
            if (kind == ParserActionKind.Fail)
            {
                Assert.AreEqual(kind, action.Kind);
            }
            else
            {
                Assert.IsNotNull(action);
                Assert.AreEqual(kind, action.Kind);
                Assert.AreEqual(value1, action.Value1);
            }

            Assert.AreEqual(kind, ParserAction.GetKind(cell));
            Assert.AreEqual(value1, ParserAction.GetId(cell));
        }

        [Theory]
        public void TestRMEncoding(
            [Values(123, ParserAction.Value1Max, 0, 1)]
            int value1, 
            [Values(123, 1, 2, ParserAction.Value2Max, 0, 1)]
            short value2)
        {
            int cell = ParserAction.EncodeModifedReduce(value1, value2);

            var output = ParserAction.Decode(cell);
            Assert.IsNotNull(output);
            Assert.AreEqual(ParserActionKind.Reduce, output.Kind);
            Assert.AreEqual(value1, output.ProductionId);
            Assert.AreEqual(value2, output.Size);
        }
    }
}
