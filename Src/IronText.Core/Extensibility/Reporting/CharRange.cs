using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Extensibility
{
    public struct CharRange
    {
        private char first;
        private char last;

        public CharRange(char first, char last)
        {
            this.first = first;
            this.last = last;
        }

        public char First { get { return first; } }

        public char Last { get { return last; } }
    }
}
