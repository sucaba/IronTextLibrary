using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IMatcherCode
    {
        IContextCode ContextCode { get; }

        /// <summary>
        /// Emit code
        /// </summary>
        /// <param name="emitPipe"></param>
        IMatcherCode Emit(Pipe<EmitSyntax> emitPipe);

        /// <summary>
        /// Load token string to stack
        /// </summary>
        IMatcherCode LdTokenString();

        /// <summary>
        /// Load buffer
        /// </summary>
        IMatcherCode LdBuffer();

        /// <summary>
        /// Load index of the token start in buffer
        /// </summary>
        IMatcherCode LdStartIndex();

        /// <summary>
        /// Load length of token
        /// </summary>
        IMatcherCode LdLength();

        /// <summary>
        /// Return top value in stack as a token value
        /// </summary>
        IMatcherCode ReturnFromAction();

        /// <summary>
        /// Skip token and continue scanning
        /// </summary>
        IMatcherCode SkipAction();

        /// <summary>
        /// Set current mode and run-time context.
        /// </summary>
        /// <remarks>
        /// Generates code which sets current context
        /// from the tompost object in stack
        /// and sets current mode (scanner DFA) determined by
        /// scanner context type. 
        /// </remarks>
        /// <param name="conditionType">Type of scanner context which determines scanner DFA to use</param>
        IMatcherCode ChangeCondition(Type conditionType);
    }
}
