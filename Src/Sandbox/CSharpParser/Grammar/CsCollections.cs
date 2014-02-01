using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public class CsCommaList<T>    : List<T> { }
    public class CsDotList<T>      : List<T> { }
    public class CsList<T>         : List<T> { }
    public class CsOptCommaList<T> : List<T> { }

    public class Opt<T> 
    {
        public Opt() { }

        public Opt(T value) { this.Value = value; }

        public T Value { get; private set; }
    }

    [Vocabulary]
    public static class CsCollections
    {
        [Produce]
        public static Opt<T> Opt<T>() 
        {
            return new Opt<T>();
        }

        [Produce]
        public static Opt<T> Opt<T>(T value) 
        {
            return new Opt<T>(value);
        }

        [Produce]
        public static CsCommaList<T> CommaList<T>(T item) 
        {
            return new CsCommaList<T> { item };
        }

        [Produce(null, ",", null)]
        public static CsCommaList<T> CommaList<T>(CsCommaList<T> list, T item) 
        { 
            list.Add(item);
            return list;
        }

        [Produce]
        public static CsDotList<T> DotList<T>(T item) 
        {
            return new CsDotList<T> { item };
        }

        [Produce(null, ".", null)]
        public static CsDotList<T> DotList<T>(CsDotList<T> list, T item) 
        { 
            list.Add(item);
            return list;
        }

        [Produce]
        public static CsList<T> List<T>(T item) 
        { 
            return new CsList<T> { item };
        }

        [Produce]
        public static CsList<T> List<T>(CsList<T> list, T item) { list.Add(item); return list; }
    }
}
