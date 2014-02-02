using System;

namespace IronText.Logging
{
    public static class LoggingExtensions
    {
        public static void Verbose(
            this ILogging   logging,
            string          origin,
            string          fmt,
            params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Origin   = origin,
                    Message  = string.Format(fmt, args)
                });
        }

        public static void WithTimeLogging(
            this ILogging logging,
            string        contextName,
            string        origin,
            Action        action,
            string        activityName)
        {
            logging.Verbose(origin, "Started {0} for {1}", activityName, contextName);

            try
            {
                action();
            }
            catch (Exception e)
            {
                logging.Write(
                    new LogEntry
                    {
                        Severity = Severity.Error,
                        Origin   = origin,
                        Message  = e.Message
                    });
            }
            finally
            {
                logging.Verbose(origin, "Done {0} for {1}", activityName, contextName);
            }
        }
    }
}
