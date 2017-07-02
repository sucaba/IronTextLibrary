using IronText.DI;
using System;
using System.Reflection;

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
            catch (InvalidDependencyException e)
            {
                throw;
            }
            catch (TargetInvocationException e) when (e.InnerException is InvalidDependencyException)
            {
                throw;
            }
            catch (TargetInvocationException e)
            {
                LogException(logging, origin, e.InnerException);
            }
            catch (Exception e)
            {
                LogException(logging, origin, e);
            }
            finally
            {
                logging.Verbose(origin, "Done {0} for {1}", activityName, contextName);
            }
        }

        private static void LogException(ILogging logging, string origin, Exception e)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Error,
                    Origin = origin,
                    Message = e.Message
                });
        }
    }
}
