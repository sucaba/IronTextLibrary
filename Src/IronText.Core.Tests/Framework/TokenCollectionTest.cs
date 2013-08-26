
namespace IronText.Tests.Framework
{
#if false
    [TestFixture]
    public class TokenCollectionTest
    {
        [Test]
        public void TypedAndLiteralAreMergedByTypedLiteral()
        {
            var target = new TokenCollection();
            target.Add(TokenIdentity.Typed(-1, typeof(string)));
            Assert.AreEqual(1, target.Count);
            target.Add(TokenIdentity.Literal(-1, "foo"));
            Assert.AreEqual(2, target.Count);
            target.Add(TokenIdentity.TypedLiteral(-1, "foo", typeof(string)));
            Assert.AreEqual(1, target.Count);
        }
    }
#endif
}
