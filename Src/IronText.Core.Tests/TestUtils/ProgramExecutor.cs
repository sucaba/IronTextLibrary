using System.Diagnostics;

namespace IronText.Tests.TestUtils
{
    public static class ProgramExecutor
    {
        public static string Execute(string compiledFileName)
        {
            var processInfo = new ProcessStartInfo(compiledFileName);
            processInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }
}
