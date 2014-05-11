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
            var writer = from c in PipeMonad.Current<StringBuilder>()
                         select c.Append("hello");

            var sb = new StringBuilder();
            writer(sb);
            Assert.AreEqual("hello", sb.ToString());
        }
    }

    public static class PipeMonad
    {
        public static Pipe<T> Current<T>() { return emit => emit; }


        public static Pipe<T> SelectMany<T>(this Pipe<T> x, Func<T,Pipe<T>> func, Func<T,T,T> select)
        {
            return x.Bind(          xval =>
                   func(xval).Bind( yval =>
                       new Pipe<T>(_ => select(xval, yval))
                   ));
        }

        public static Pipe<T> Select<T>(this Pipe<T> a, Func<T,T> select)
        {
            return a.Bind(x => _ => select(x));
        }

        private static Pipe<T> Bind<T>(this Pipe<T> a, Func<T, Pipe<T>> func)
        {
            return emit => func(a(emit))(default(T));
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
