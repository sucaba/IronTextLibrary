using IronText.Lib.Sre;
using NUnit.Framework;

namespace IronText.Tests.Lib.NfaVM
{
    [TestFixture]
    public class PikeVMTest : SreTestBase
    {
        protected override SRegex MakeSRegex(string pattern)
        {
            return new SRegex(pattern, SRegexOptions.ByteCodeCompilation);
        }
    }
}
