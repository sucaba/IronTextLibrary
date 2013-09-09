using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samples
{
    class Program
    {
        public static void Main()
        {
            var config = new MyConfig();
            config.Parse(
                @"
                // Window parameters:
                Width  = 12
                Heigth = 300
                Title  = ""Hello world""

                /* Some additional constants */
                Pi     = 3.14
                ");

            foreach (var pair in config.Parameters)
            {
                Console.WriteLine("{0} = {1}", pair.Key, pair.Value);
            }
        }
    }
}
