using System.IO;

namespace IronText.Tests.Extensions
{
    static class StringExtensions
    {
        public static string Times(this string pattern, int times)
        {
            using (var writer = new StringWriter())
            {
                pattern.Times(times, writer);

                return writer.ToString();
            }
        }

        public static void Times(this string pattern, int times, TextWriter writer)
        {
            for (int count = times; count-- != 0;)
            {
                writer.Write(pattern);
            }
        }
    }
}
