using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IMergeActionCode
    {
        IContextResolverCode ContextResolver { get; }

        IMergeActionCode Emit(Pipe<EmitSyntax> emit);

        IMergeActionCode LdOldValue();

        IMergeActionCode LdNewValue();
    }
}
