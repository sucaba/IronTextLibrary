using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    public delegate void Fluent<T>(Pipe<T> action);

    /// <summary>
    /// Utility class which encapsulate fluent syntax object to avoid forking 
    /// and using wrong versions of current state represented by syntax.
    /// </summary>
    /// <typeparam name="TSyntax"></typeparam>
    public static class Fluent
    {
        public static Fluent<T> Create<T>(T current) where T : class
        {
            return (Pipe<T> action) => { current = action(current); };
        }

        public static void Do<T>(this Fluent<T> fluent, Pipe<T> action)
        {
            fluent(action);
        }
    }
}
