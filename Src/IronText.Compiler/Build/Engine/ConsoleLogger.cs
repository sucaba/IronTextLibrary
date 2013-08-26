using System;

namespace IronText.Build
{
    class ConsoleLogger : ILogger
    {
        public void LogError(string fmt, params object[] args)
        {
            Console.WriteLine("error: " + fmt, args);
        }

        public void LogErrorFromException(Exception e)
        {
            Console.WriteLine("error: " + e.Message);
        }

        public void LogWarning(string fmt, params object[] args)
        {
            Console.WriteLine("warning: " + fmt, args);
        }

        public void LogMessage(string fmt, params object[] args)
        {
            Console.WriteLine("message: " + fmt, args);
        }

        public void LogVerbose(string fmt, params object[] args)
        {
#if DIAGNOSTICS
            Console.WriteLine("diagnostics: " + fmt, args);
#endif
        }
    }
}
