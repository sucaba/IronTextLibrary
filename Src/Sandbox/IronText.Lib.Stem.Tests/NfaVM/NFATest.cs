using IronText.Lib.Sre;
using NUnit.Framework;

namespace IronText.Tests.Lib.NfaVM
{
    [TestFixture]
    public class NFATest : SreTestBase
    {
        protected override SRegex MakeSRegex(string pattern)
        {
            return new SRegex(pattern, SRegexOptions.NfaCompilation);
        }
    }
}
