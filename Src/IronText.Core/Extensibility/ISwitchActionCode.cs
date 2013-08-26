using IronText.Framework;
using IronText.Lib.IL;

namespace IronText.Extensibility
{
    public interface ISwitchActionCode
    {
        /// <summary>
        /// Context resolver
        /// </summary>
        IContextResolverCode ContextResolver { get; }

        /// <summary>
        /// Emit native IL code
        /// </summary>
        /// <param name="emit"></param>
        /// <returns></returns>
        ISwitchActionCode Emit(Pipe<EmitSyntax> emit);

        /// <summary>
        /// Load language instance
        /// </summary>
        /// <returns></returns>
        ISwitchActionCode LdLanguage();

        /// <summary>
        /// Load exit-receiver instance
        /// </summary>
        /// <returns></returns>
        ISwitchActionCode LdExitReceiver();
    }
}
