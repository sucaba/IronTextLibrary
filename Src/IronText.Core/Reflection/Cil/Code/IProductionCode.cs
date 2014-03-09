using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IProductionCode
    {
        IContextCode ContextResolver { get; }

        IProductionCode Emit(Pipe<EmitSyntax> emit);

        IProductionCode LdRuleArg(int index);
        IProductionCode LdRuleArg(int index, Type argType);
    }
}
