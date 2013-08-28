
namespace IronText.Framework
{

    public interface IPushParser : IReceiver<Msg>
    {
        /// <summary>
        /// Creates parser without side effects but with
        /// the same state as original parser.
        /// </summary>
        /// <returns>
        /// Parser without side-effects.
        /// </returns>
        IPushParser CloneVerifier();

        IReceiver<Msg> ForceNext(params Msg[] msg);
    }
}
