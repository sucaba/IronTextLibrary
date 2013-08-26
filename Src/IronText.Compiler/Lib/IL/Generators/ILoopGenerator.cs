
namespace IronText.Lib.IL.Generators
{
    public interface ILoopGenerator
    {
        EmitSyntax EmitInitialization(EmitSyntax emit);
        EmitSyntax EmitLoopPass(EmitSyntax emit, bool loop);
    }
}
