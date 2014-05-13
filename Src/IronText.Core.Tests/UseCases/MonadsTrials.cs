using IronText.Framework;
using IronText.Lib.IL;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Tests.UseCases
{
    [TestFixture]
    public class MonadsTrials
    {
        [Test]
        public void Test()
        {
            MayBe<int> x = new MayBe<int>(4);                
            MayBe<int> y = new MayBe<int>(3);                
            MayBe<int> z = new MayBe<int>();                

            var val1 = (from a in x from b in y select a + b);
            Assert.AreEqual(7, val1.Value);
            Assert.IsTrue(val1.HasValue);

            var val2 = (from a in x from b in z select a + b);
            Assert.IsFalse(val2.HasValue);
        }

        [Test]
        public void TestPipes()
        {
            Func<StringBuilder,Func<StringBuilder>> writer = sbp => 
                         from c in PipeMonad.Current(sbp)
                         let n = c.Append("prefix").Length
                         select c.Append("hello").Append(n);

            var sb = new StringBuilder();
            writer(sb)();
            Assert.AreEqual("hello", sb.ToString());
        }
    }

    public static class PipeMonad
    {
        public static Func<T> Current<T>(T val) { return () => val; }


        public static Func<R> SelectMany<T,U,R>(this Func<T> x, Func<T,Func<U>> func, Func<T,U,R> select)
        {
            return x.Bind(          xval =>
                   func(xval).Bind( yval =>
                       new Func<R>(() => select(xval, yval))
                   ));
        }

        public static Func<U> Select<T,U>(this Func<T> a, Func<T,U> select)
        {
            return a.Bind(x => new Func<U>(() => select(x)));
        }

        private static Func<U> Bind<T,U>(this Func<T> a, Func<T, Func<U>> func)
        {
            return () => func(a())();
        }
    }

    public static class MayBe
    {
        public static MayBe<T> None<T>() {  return new MayBe<T>(); }

        public static MayBe<T> Some<T>(this T value)
        {
            return new MayBe<T>(value);
        }

        public static MayBe<T> SelectMany<T>(this MayBe<T> x, Func<T,MayBe<T>> func, Func<T,T,T> select)
        {
            return x.Bind(          xval =>
                   func(xval).Bind( yval =>
                       new MayBe<T>(select(xval, yval))
                   ));
        }

        public static MayBe<T> Bind<T>(this MayBe<T> a, Func<T, MayBe<T>> func)
        {
            if (a.HasValue)
            {
                return func(a.Value);
            }

            return None<T>();
        }
    }

    public class MayBe<T>
    {
        public MayBe(T value)
        {
            this.HasValue = true;
            this.Value = value;
        }

        public MayBe() { }

        public bool HasValue {  get; private set; }
    
        public  T Value { get; private set; }
    }
}
