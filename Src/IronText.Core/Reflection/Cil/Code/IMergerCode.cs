using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IMergerCode
    {
        IContextCode ContextResolver { get; }

        IMergerCode Emit(Pipe<EmitSyntax> emit);

        IMergerCode LdOldValue();

        IMergerCode LdNewValue();
    }
}
