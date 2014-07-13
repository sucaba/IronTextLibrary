using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Misc
{
    internal class IdentityFactory
    {
        public static object FromString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            return s;
        }

        public static object FromStrings(IEnumerable<string> secondary)
        {
            return string.Join(" ", secondary);
        }

        public static object FromStrings(string main, IEnumerable<string> secondary)
        {
            return main + string.Join(" ", secondary);
        }

        public static object FromIdentities(IHasIdentity main, IEnumerable<IHasIdentity> secondary)
        {
            if (main == null)
            {
                throw new ArgumentNullException("main");
            }

            if (secondary == null)
            {
                throw new ArgumentNullException("secondary");
            }

            return main.Identity + "\n" + string.Join("\n", secondary.Select(id => id.Identity));
        }

        public static object FromObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj;
        }

        public static object FromIntegers(IEnumerable<int> tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            return FromStrings(tokens.Select(tok => tok.ToString()));
        }
    }
}
