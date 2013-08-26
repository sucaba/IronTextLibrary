using System;
using IronText.Extensibility;
using NUnit.Framework;

namespace IronText.Tests.Extensions
{
    [TestFixture]
    public class TokenRefResolverTest
    {
        TokenRefResolver target;

        [SetUp]
        public void SetUp()
        {
            target = new TokenRefResolver();
        }

        [Test]
        public void Test()
        {
            var tokens = new[] {
                TokenRef.Literal("foo"),
                TokenRef.Literal("bar"),
                TokenRef.Literal("other"),
                TokenRef.Typed(typeof(string)),
            };

            foreach (var token in tokens)
            {
                target.Link(token);
            }

            target.Link(tokens);

            const int ID = 123;
            target.SetId(tokens[0], ID);
            for (int i = 1; i != tokens.Length; ++i)
            {
                var context = "#" + i;
                Assert.AreEqual(ID, target.GetId(tokens[i]), context);
            }
        }

        [Test]
        [ExpectedException(
            ExpectedException=typeof(InvalidOperationException),
            MatchType = MessageMatch.Contains,
            ExpectedMessage = "two types")]
        public void TestLinkTwoTypes()
        {
            target.Link(TokenRef.Typed(typeof(string)), TokenRef.Typed(typeof(int)));
        }

        [Test]
        [ExpectedException(
            ExpectedException=typeof(InvalidOperationException),
            MatchType = MessageMatch.Contains,
            ExpectedMessage = "two types")]
        public void TestIndirectLinkTwoTypes()
        {
            target.Link(TokenRef.Typed(typeof(string)), TokenRef.Literal("foo"));
            target.Link(TokenRef.Typed(typeof(int)), TokenRef.Literal("foo"));
        }

        [Test]
        [ExpectedException(
            ExpectedException=typeof(InvalidOperationException),
            MatchType = MessageMatch.Contains,
            ExpectedMessage = "two types")]
        public void TestIndirectLinkOfExistingDefs()
        {
            target.Link(TokenRef.Typed(typeof(string)), TokenRef.Literal("foo"));
            target.Link(TokenRef.Typed(typeof(int)), TokenRef.Literal("bar"));

            target.Link(TokenRef.Literal("foo"), TokenRef.Literal("bar"));
        }
    }
}
