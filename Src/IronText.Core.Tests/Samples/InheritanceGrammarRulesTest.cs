using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib;
using IronText.Lib.Ctem;
using NUnit.Framework;

namespace IronText.Tests.Samples
{
    [TestFixture]
    public class CompositeDataTest
    {
        [Test]
        public void Test()
        {
            var got = Language.Parse(
                new AstLang(),
                "(myid \"my str\" (my_id_2 \"my str 2\") () )")
                .Result;

            var expected = new StxComposite
                (
                    new StxNode[]
                    {
                        new StxLeaf<string>("myid"),
                        new StxLeaf<QStr>(new QStr("my str")),
                        new StxComposite
                        (
                            new StxNode[]
                            {
                                new StxLeaf<string>("my_id_2"),
                                new StxLeaf<QStr>(new QStr("my str 2")),
                            }
                        ),
                        new StxComposite ( new StxNode[0] ),
                    }
                );

            Assert.AreEqual(expected, got);
        }

        public abstract class StxNode { }

        public class StxLeaf<T> : StxNode
        {
            private readonly T _value;

            public StxLeaf(T value)
            {
                this._value = value;
            }

            public override bool Equals(object obj)
            {
                var casted = obj as StxLeaf<T>;
                bool result = casted != null && object.Equals(casted._value, _value);
                return result;
            }

            public override int GetHashCode() { return _value.GetHashCode(); }
        }

        public class StxComposite : StxNode
        {
            private readonly List<StxNode> _children;

            public StxComposite(IEnumerable<StxNode> items)
            {
                _children = new List<StxNode>(items);
            }

            public override bool Equals(object obj)
            {
                var casted = obj as StxComposite;
                bool result = casted != null && Enumerable.SequenceEqual(casted._children, _children);
                return result;
            }

            public override int GetHashCode() { return base.GetHashCode(); }
        }

        [Language]
        [StaticContext(typeof(Builtins))]
        public class AstLang
        {
            public AstLang()
            {
                this.Scanner = new CtemScanner();
            }

            public StxNode Result { get; [Parse] set; }

            [SubContext]
            public CtemScanner Scanner { get; private set; }

            [Parse("(", null, ")")]
            public StxNode Branch(StxNode[] children) { return new StxComposite(children); }

            [Parse("(", ")")]
            public StxNode EmptyBranch() { return new StxComposite(new StxNode[0]); }

            [Parse]
            public StxNode Leaf(QStr str) { return new StxLeaf<QStr>(str); }

            [Parse]
            public StxNode Leaf(string idn) { return new StxLeaf<string>(idn); }
        }
    }
}
