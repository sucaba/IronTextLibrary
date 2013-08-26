using NUnit.Framework;

namespace IronText.Tests.Lib.Scope
{
    [TestFixture]
    public class NsBaseTest
    {
        [Test]
        public void Test()
        {
#if false
            object cellValue = new object();
            var name = new Idn("foo");
            var target = new Ns<object>();
            target.PushFrame();
            var defineResult = target.Define(name);
            var getResult = target.Get(name);
            Assert.AreSame(defineResult, getResult);
            Assert.AreSame(cellValue, getResult);
            target.PopFrame();
#endif
        }
    }
}
