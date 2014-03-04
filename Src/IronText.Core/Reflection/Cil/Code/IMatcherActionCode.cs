using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Reflection.Managed
{
    public interface IMatcherActionCode
    {
        IContextResolverCode ContextResolver { get; }

        /// <summary>
        /// Emit code
        /// </summary>
        /// <param name="emitPipe"></param>
        IMatcherActionCode Emit(Pipe<EmitSyntax> emitPipe);

        /// <summary>
        /// Load token string to stack
        /// </summary>
        IMatcherActionCode LdTokenString();

        /// <summary>
        /// Load buffer
        /// </summary>
        IMatcherActionCode LdBuffer();

        /// <summary>
        /// Load index of the token start in buffer
        /// </summary>
        IMatcherActionCode LdStartIndex();

        /// <summary>
        /// Load length of token
        /// </summary>
        IMatcherActionCode LdLength();

        /// <summary>
        /// Return top value in stack as a token value
        /// </summary>
        IMatcherActionCode ReturnFromAction();

        /// <summary>
        /// Skip token and continue scanning
        /// </summary>
        IMatcherActionCode SkipAction();

        /// <summary>
        /// Set current mode and run-time context.
        /// </summary>
        /// <remarks>
        /// Generates code which sets current context
        /// from the tompost object in stack
        /// and sets current mode (scanner DFA) determined by
        /// scanner context type. 
        /// </remarks>
        /// <param name="modeType">Type of scanner context which determines scanner DFA to use</param>
        IMatcherActionCode ChangeMode(Type modeType);
    }
}
