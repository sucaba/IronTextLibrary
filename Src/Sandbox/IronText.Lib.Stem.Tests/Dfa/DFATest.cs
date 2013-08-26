using IronText.Lib.Sre;
using IronText.Tests.Lib.NfaVM;
using NUnit.Framework;

namespace IronText.Tests.Lib.Sre
{
    [TestFixture]
    public class DFATest : SreTestBase
    {
        protected override SRegex MakeSRegex(string pattern)
        {
            return new SRegex(pattern, SRegexOptions.DfaCompilation);
        }
    }
}
