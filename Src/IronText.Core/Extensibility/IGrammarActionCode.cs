using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Extensibility
{
    public interface IGrammarActionCode
    {
        IContextResolverCode ContextResolver { get; }

        IGrammarActionCode Emit(Pipe<EmitSyntax> emit);

        IGrammarActionCode LdRuleArg(int index);
        IGrammarActionCode LdRuleArg(int index, Type argType);
    }
}
