using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IActionCode
    {
        IActionCode Emit(Pipe<EmitSyntax> emit);

        IActionCode LdSemantic(string name);

        IActionCode LdActionArgument(int index);

        IActionCode LdActionArgument(int index, Type argType);

        IActionCode LdMergerOldValue();

        IActionCode LdMergerNewValue();

        /// <summary>
        /// Load token string to stack
        /// </summary>
        IActionCode LdMatcherTokenString();

        /// <summary>
        /// Return top value in stack as a token value
        /// </summary>
        IActionCode ReturnFromAction();

        /// <summary>
        /// Skip token and continue scanning
        /// </summary>
        IActionCode SkipAction();
    }

    public static class ActionCodeExtensions
    {
        public static IActionCode Do(this IActionCode self, Pipe<IActionCode> builder)
        {
            return builder(self);
        }
    }
}
