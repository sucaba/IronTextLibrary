using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Framework
{
    /// <summary>
    /// Utility class which encapsulate fluent syntax object to avoid forking 
    /// and using wrong versions of current state represented by syntax.
    /// </summary>
    /// <typeparam name="TSyntax"></typeparam>
    public sealed class Fluent<TSyntax> where TSyntax : class
    {
        private TSyntax current;

        public Fluent(TSyntax initial)
        {
            this.current = initial;
        }

        public void Do(Pipe<TSyntax> pipe)
        {
            current = pipe(current);
        }
    }
}
