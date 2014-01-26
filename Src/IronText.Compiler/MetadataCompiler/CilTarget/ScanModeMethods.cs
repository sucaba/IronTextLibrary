
namespace IronText.MetadataCompiler
{
    static class ScanModeMethods
    {
        private static string Scan1MethodNameFormat = "ScanMode_{0}";

        public static string GetMethodName(int modeIndex)
        {
            var result = string.Format(Scan1MethodNameFormat, modeIndex);
            return result;
        }
    }
}
