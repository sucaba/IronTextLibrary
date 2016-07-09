
namespace IronText.Runtime
{
    public interface IPushParser : IReceiver<Message>
    {
        /// <summary>
        /// Creates parser without side effects but with
        /// the same state as original parser.
        /// </summary>
        /// <returns>
        /// Parser without side-effects.
        /// </returns>
        IPushParser CloneVerifier();

        IReceiver<Message> ForceNext(params Message[] msg);
    }
}
