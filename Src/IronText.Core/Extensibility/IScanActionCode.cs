using System;
using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Extensibility
{
    public interface IScanActionCode
    {
        IContextResolverCode ContextResolver { get; }

        /// <summary>
        /// Emit code
        /// </summary>
        /// <param name="emitPipe"></param>
        IScanActionCode Emit(Pipe<EmitSyntax> emitPipe);

        /// <summary>
        /// Load token string to stack
        /// </summary>
        IScanActionCode LdTokenString();

        /// <summary>
        /// Load buffer
        /// </summary>
        IScanActionCode LdBuffer();

        /// <summary>
        /// Load index of the token start in buffer
        /// </summary>
        IScanActionCode LdStartIndex();

        /// <summary>
        /// Load length of token
        /// </summary>
        IScanActionCode LdLength();

        /// <summary>
        /// Return top value in stack as a token value
        /// </summary>
        IScanActionCode ReturnFromAction();

        /// <summary>
        /// Skip token and continue scanning
        /// </summary>
        IScanActionCode SkipAction();

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
        IScanActionCode ChangeMode(Type modeType);
    }
}
