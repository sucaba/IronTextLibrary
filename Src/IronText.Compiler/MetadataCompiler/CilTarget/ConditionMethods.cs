
namespace IronText.MetadataCompiler
{
    static class ConditionMethods
    {
        private static string Scan1MethodNameFormat = "Condition_{0}";

        public static string GetMethodName(int modeIndex)
        {
            var result = string.Format(Scan1MethodNameFormat, modeIndex);
            return result;
        }
    }
}
