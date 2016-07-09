using IronText.Logging;
using System.Collections.Generic;

namespace IronText.Runtime
{
    class PanicModeErrorRecovery : IReceiver<Message>
    {
        private readonly RuntimeGrammar grammar;
        private readonly IPushParser exit;
        private readonly ILogging logging;
        private Loc errorLocation = Loc.Unknown;
        private readonly List<Message> collectedInput = new List<Message>();
        private IReceiver<Message> validPrefixVerifier;

        public PanicModeErrorRecovery(RuntimeGrammar grammar, IPushParser exit, ILogging logging)
        {
            this.grammar = grammar;
            this.exit    = exit;
            this.logging = logging;
            this.validPrefixVerifier = exit.CloneVerifier();
        }

        public IReceiver<Message> Next(Message item)
        {
            // Skip valid tokens before the error which were used for local error recovery.
            if (validPrefixVerifier != null)
            {
                validPrefixVerifier = validPrefixVerifier.Next(item);
                if (validPrefixVerifier != null)
                {
                    return this;
                }
            }

            var error = new Message(PredefinedTokens.Error, null, null, errorLocation); // TODO: Location?
            if (null != exit.CloneVerifier().ForceNext(error, item))
            {
                ReportError();
                return exit.ForceNext(error, item);
            }
            else if (grammar.IsBeacon(item.AmbiguousToken)
                    && null != exit.CloneVerifier().ForceNext(item))
            {
                ReportError();
                return exit.ForceNext(item);
            }

            errorLocation += item.Location;
            collectedInput.Add(item);
            return this;
        }

        private void ReportError()
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Location = errorLocation,
                    Message = "Unexpected input"
                });
        }

        public IReceiver<Message> Done()
        {
            ReportError();
            return null;
        }
    }
}
