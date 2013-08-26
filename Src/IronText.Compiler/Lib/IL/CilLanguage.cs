using IronText.Lib.IL.Backend.Cecil;

namespace IronText.Lib.IL
{
    public static class CilLanguage
    {
        public static CilSyntax CreateCompiler(string documentPath)
        {
            return CecilBackend.Create(documentPath);
        }
    }
}
