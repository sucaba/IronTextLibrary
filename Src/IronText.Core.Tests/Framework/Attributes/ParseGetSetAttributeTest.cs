using IronText.Framework;
using IronText.Runtime;
using NUnit.Framework;

namespace IronText.Tests.Framework.Attributes
{
    [TestFixture]
    public class ParseGetSetAttributeTest
    {
        [Test]
        public void Test()
        {
            var context = new ParseGetSetTestLang();
            Language.Parse(context, "");
            Assert.AreEqual("empty", context.Result.Name);

            Language.Parse(context, "literal");
            Assert.AreEqual("literal", context.Result.Name);
        }

        [Language]
        public class ParseGetSetTestLang
        {
            [Outcome]
            public Tag Result { get; set; }

            [ParseGet]
            public Tag Produce { get { return new Tag("empty"); } }

            [ParseGet("literal")]
            public Tag ProduceLiteral { get { return new Tag("literal"); } }

            [Match("blank+")]
            public void Blank () {}
        }

        public class Tag
        {
            public readonly string Name;

            public Tag(string name) { this.Name = name; }

            public override string ToString() { return Name; }
        }
    }
}
