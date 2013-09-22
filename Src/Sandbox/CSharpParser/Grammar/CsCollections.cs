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
    public class CsOptList<T>      : List<T> { }
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
        [Parse]
        public static Opt<T> Opt<T>() 
        {
            return new Opt<T>();
        }

        [Parse]
        public static Opt<T> Opt<T>(T value) 
        {
            return new Opt<T>(value);
        }

        [Parse]
        public static CsOptList<T> OptCsList<T>() 
        {
            return new CsOptList<T>();
        }

        [Parse]
        public static CsOptList<T> OptCsList<T>(CsOptList<T> list, T value) 
        {
            list.Add(value);
            return list;
        }

        [Parse]
        public static CsCommaList<T> CommaList<T>(T item) 
        {
            return new CsCommaList<T> { item };
        }

        [Parse(null, ",", null)]
        public static CsCommaList<T> CommaList<T>(CsCommaList<T> list, T item) 
        { 
            list.Add(item);
            return list;
        }

        [Parse]
        public static CsOptCommaList<T> OptCommaList<T>() 
        {
            return new CsOptCommaList<T>();
        }

        [Parse(null, ",", null)]
        public static CsOptCommaList<T> CommaList<T>(CsOptCommaList<T> list, T item) 
        { 
            list.Add(item);
            return list;
        }

        [Parse]
        public static CsDotList<T> DotList<T>(T item) 
        {
            return new CsDotList<T> { item };
        }

        [Parse(null, ".", null)]
        public static CsDotList<T> DotList<T>(CsDotList<T> list, T item) 
        { 
            list.Add(item);
            return list;
        }

        [Parse]
        public static CsList<T> List<T>(T item) 
        { 
            return new CsList<T> { item };
        }

        [Parse]
        public static CsList<T> List<T>(CsList<T> list, T item) { list.Add(item); return list; }
    }
}
