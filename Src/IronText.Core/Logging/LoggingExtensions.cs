using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IronText.Framework;

namespace IronText.Logging
{
    public static class LoggingExtensions
    {
        public static void Verbose(
            this ILogging   logging,
            MemberInfo      member,
            string          fmt,
            params object[] args)
        {
            logging.Write(
                new LogEntry
                {
                    Severity = Severity.Verbose,
                    Member   = member,
                    Message  = string.Format(fmt, args)
                });
        }

        public static void WithTimeLogging(
            this ILogging logging,
            string        contextName,
            MemberInfo    member,
            Action        action,
            string        activityName)
        {
            logging.Verbose(member, "Started {0} for {1}", activityName, contextName);

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
                        Member   = member,
                        Message  = e.Message
                    });
            }
            finally
            {
                logging.Verbose(member, "Done {0} for {1}", activityName, contextName);
            }
        }
    }
}
