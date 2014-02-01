using System.Linq;
using System.Reflection;
using IronText.Framework;
using NUnit.Framework;

namespace IronText.Tests.Framework.Attributes
{
    [TestFixture]
    public class MetaAttributeTests
    {
        [Test]
        public void Test()
        {
            var x = new MatchAttribute("foo");   x.Bind(null, typeof(string));
            var x2 = new MatchAttribute("foo"); x2.Bind(null, typeof(string));
            var y = new MatchAttribute("foo");   y.Bind(null, typeof(object));
            var y2 = new MatchAttribute("bar"); y2.Bind(null, typeof(string));

            var type = typeof(ScanBaseAttribute);
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            string[] fieldNames = fields.Select(f => f.Name).ToArray();
            Assert.GreaterOrEqual(fields.Length, 2);
  
            Assert.AreEqual(x, x);
            Assert.AreEqual(x, x2);
            Assert.AreNotEqual(x, y);
            Assert.AreNotEqual(x, y2);
        }
    }
}
