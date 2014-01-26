using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IProductionActionCode
    {
        IContextResolverCode ContextResolver { get; }

        IProductionActionCode Emit(Pipe<EmitSyntax> emit);

        IProductionActionCode LdRuleArg(int index);
        IProductionActionCode LdRuleArg(int index, Type argType);
    }
}
