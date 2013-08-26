using System;
using System.Reflection;
using IronText.Build;
using IronText.Lib.IL.Generators;
using IronText.Misc;
using NUnit.Framework;

namespace IronText.Tests.Lib.IL.Generators
{
    [TestFixture]
    public class ImplementationGeneratorTest
    {
        [Test]
        public void TestInterfaceImplementation()
        {
            var target = new ImplementationGenerator(
                                MethodInfo.GetCurrentMethod().Name,
                                CanReturnThis);

            target.PlanImplementationOf(typeof(ITestRoot));

            Assembly assembly;
            Assert.IsTrue(ResourceContext.Instance.LoadOrBuild(target, out assembly));

            string className = target.PlannedClassNames[0];
            var inst = (ITestRoot)Activator.CreateInstance(assembly.GetType(className));
            Assert.AreEqual(0, inst.ValueReturningMethod2(null));

            Assert.AreEqual(0, inst.ValueProperty2);
            inst.ValueProperty2 = 555;
            Assert.AreEqual(0, inst.ValueProperty2);

            Assert.IsNull(inst.ObjectReturningMethod2(null));

            Assert.IsNull(inst.ObjectProperty2);
            
            Assert.IsNull(inst.ObjectReturningMethod2(null));

            Assert.AreEqual(0, inst.ValueProperty0);

            Assert.AreEqual(0, inst.ValueProperty1);

            Assert.AreSame(inst, inst.AspectMethod1(null));

            Assert.AreEqual(0, inst.AspectMethod1(null).ValueProperty3);
        }

        [Test]
        public void TestAbstractClassImplementation()
        {
            var target = new ImplementationGenerator(
                                MethodInfo.GetCurrentMethod().Name,
                                CanReturnThis);

            target.PlanImplementationOf(typeof(ATestRoot));
            Assembly assembly;
            Assert.IsTrue(ResourceContext.Instance.LoadOrBuild(target, out assembly));

            string className = target.PlannedClassNames[0];
            var inst = (ATestRoot)Activator.CreateInstance(assembly.GetType(className));

            Assert.AreEqual(0, inst.ValueReturningMethod2(null));

            Assert.AreEqual(0, inst.ValueProperty2);
            inst.ValueProperty2 = 555;
            Assert.AreEqual(0, inst.ValueProperty2);

            Assert.IsNull(inst.ObjectReturningMethod2(null));

            Assert.IsNull(inst.ObjectProperty2);
            
            Assert.IsNull(inst.ObjectReturningMethod2(null));

            Assert.AreEqual(0, inst.ValueProperty0);

            Assert.AreEqual(0, inst.ValueProperty1);

            Assert.AreSame(inst, inst.AspectMethod1(null));

            Assert.IsNotNull(inst.TestResult1(null));

            Assert.IsNull(inst.TestResult2(null));

            Assert.AreSame(inst, inst.TestResult3(null));

            Assert.IsNull(inst.TestResult4(null));
        }

        public interface ITestRoot0
        {
            int ValueProperty0 { get; }
        }

        public interface ITestRoot1 : ITestRoot0
        {
            int ValueProperty1 { get; }
        }

        public interface ITestRoot : ITestRoot1
        {
            int ValueProperty2 { get; set; }

            object ObjectProperty2 { get; }

            int ValueReturningMethod2(string name);

            string ObjectReturningMethod2(string name);

            void VoidMethod2(string name);

            [ReturnThis]
            IAspect1 AspectMethod1(string name);
        }

        public interface IAspect1
        {
            int ValueProperty3 { get; set; }
        }

        public class ReturnThisAttribute : Attribute
        {
        }

        public abstract class ATestRoot0
        {
            public abstract int ValueProperty0 { get; set; }
        }

        public abstract class ATestRoot1 : ATestRoot0
        {
            public abstract int ValueProperty1 { get; set; }
        }

        public abstract class ATestRoot : ATestRoot1
        {
            public abstract int ValueProperty2 { get; set; }

            public abstract object ObjectProperty2 { get; }

            public abstract int ValueReturningMethod2(string name);

            public abstract string ObjectReturningMethod2(string name);

            public abstract void VoidMethod2(string name);

            [ReturnThis]
            public abstract IAspect1 AspectMethod1(string name);

            [ReturnThis]
            public abstract ATestResult1 TestResult1(string name);

            // Same as previous but now should return null
            public abstract ATestResult1 TestResult2(string name);

            // Should return this
            [ReturnThis]
            public abstract ATestRoot0 TestResult3(string name);

            // Same as previous but now should return null
            public abstract ATestRoot0 TestResult4(string name);
        }

        public abstract class ATestResult1
        {
            public abstract int ValueProperty3 { get; set; }
        }

        private static bool CanReturnThis(MethodInfo method)
        {
            return Attributes.Exists<ReturnThisAttribute>(method);
        }

    }
}
