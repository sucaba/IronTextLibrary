using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IActionCode
    {
        IActionCode Emit(Pipe<EmitSyntax> emit);

        IActionCode LdContext(string contextName);

        IActionCode LdActionArgument(int index, Type argType);

        IActionCode LdMergerOldValue();

        IActionCode LdMergerNewValue();

        /// <summary>
        /// Load token string to stack
        /// </summary>
        IActionCode LdMatcherTokenString();

        /// <summary>
        /// Load buffer
        /// </summary>
        IActionCode LdMatcherBuffer();

        /// <summary>
        /// Load index of the token start in buffer
        /// </summary>
        IActionCode LdMatcherStartIndex();

        /// <summary>
        /// Load length of token
        /// </summary>
        IActionCode LdMatcherLength();

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
