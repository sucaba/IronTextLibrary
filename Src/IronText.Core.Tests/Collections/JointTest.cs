using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Collections;
using NUnit.Framework;

namespace IronText.Tests.Collections
{
    [TestFixture]
    public class JointTest
    {
        [Test]
        public void SetGetSingleTest()
        {
            var target = new Joint();
            var inst = new TestService();

            target.Add<TestService>(inst);

            Assert.IsTrue(target.Has<TestService>());
            Assert.AreSame(inst, target.The<TestService>());
            Assert.AreSame(inst, target.Get<TestService>());

            Assert.IsTrue(target.Has<ITestService>());
            Assert.AreSame(inst, target.The<ITestService>());
        }

        [Test]
        public void SetGetMultipleTest()
        {
            var target = new Joint();
            var instances = new[] { new TestService(), new TestService(), new TestService() };

            foreach (var inst in instances)
            {
                target.Add<TestService>(inst);
            }

            Assert.IsTrue(target.Has<TestService>());
            Assert.AreEqual(instances, target.All<TestService>().ToArray());
            Assert.AreEqual(instances, target.All<ITestService>().ToArray());

            Assert.Throws<InvalidOperationException>(() => target.The<TestService>());
            Assert.Throws<InvalidOperationException>(() => target.The<ITestService>());
            Assert.Throws<InvalidOperationException>(() => target.Get<ITestService>());
            Assert.Throws<InvalidOperationException>(() => target.Get<TestService>());
        }

        [Test]
        public void NullCanBeAddedAndHasNoEffect()
        {
            var target = new Joint();
            target.Add(null);
            Assert.IsFalse(target.Has<object>());
            Assert.IsNull(target.Get<object>());
            Assert.Throws<InvalidOperationException>(() => target.The<object>());
        }

        public interface ITestService { }

        public class TestService : ITestService { }
    }
}
