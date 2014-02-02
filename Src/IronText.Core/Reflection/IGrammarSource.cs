using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Reflection
{
    public interface IGrammarSource
    {
        string LanguageName      { get; }

        string FullLanguageName  { get; }

        string Origin            { get; }
    }
}
