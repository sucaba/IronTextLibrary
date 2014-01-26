using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace IronText.Lib.IL
{
    class TypeSig
    {
        public static void SplitFullName (string fullname, out string @namespace, out string name)
        {
            var lastDot = fullname.LastIndexOf('.');

            if (lastDot == -1)
            {
                @namespace = string.Empty;
                name = fullname;
            }
            else
            {
                @namespace = fullname.Substring (0, lastDot);
                name = fullname.Substring (lastDot + 1);
            }
        }
    }

}
