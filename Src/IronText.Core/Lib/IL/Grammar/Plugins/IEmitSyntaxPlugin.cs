
namespace IronText.Lib.IL
{
    public interface IEmitSyntaxPlugin
    {
        void SetCurrent(EmitSyntax emit);

        void Init(ICilDocumentInfo documentInfo);

        CilDocumentSyntax BeforeEndDocument(CilDocumentSyntax syntax);
    }
}
