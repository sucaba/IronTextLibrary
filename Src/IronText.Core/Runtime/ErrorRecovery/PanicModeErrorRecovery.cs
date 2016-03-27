using IronText.Logging;
using System.Collections.Generic;

namespace IronText.Runtime
{
    class PanicModeErrorRecovery : IReceiver<Msg>
    {
        private readonly RuntimeGrammar grammar;
        private readonly IPushParser exit;
        private readonly ILogging logging;
        private HLoc errorHLocation = HLoc.Unknown;
        private readonly List<Msg> collectedInput = new List<Msg>();
        private IReceiver<Msg> validPrefixVerifier;

        public PanicModeErrorRecovery(RuntimeGrammar grammar, IPushParser exit, ILogging logging)
        {
            this.grammar = grammar;
            this.exit    = exit;
            this.logging = logging;
            this.validPrefixVerifier = exit.CloneVerifier();
        }

        public IReceiver<Msg> Next(Msg item)
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

            var error = new Msg(PredefinedTokens.Error, null, null, errorHLocation); // TODO: Location?
            if (null != exit.CloneVerifier().ForceNext(error, item))
            {
                ReportError();
                return exit.ForceNext(error, item);
            }
            else if (grammar.IsBeacon(item.AmbToken)
                    && null != exit.CloneVerifier().ForceNext(item))
            {
                ReportError();
                return exit.ForceNext(item);
            }

            errorHLocation += item.HLocation;
            collectedInput.Add(item);
            return this;
        }

        private void ReportError()
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    HLocation = errorHLocation,
                    Message = "Unexpected input"
                });
        }

        public IReceiver<Msg> Done()
        {
            ReportError();
            return null;
        }
    }
}
