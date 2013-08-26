using System;
using System.IO;

namespace NDerive
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = new FileInfo(args[0]);
            var output
                = new FileInfo(
                    Path.Combine(
                        Path.GetDirectoryName(input.FullName),
                        Path.GetFileNameWithoutExtension(input.FullName) + ".Merged"
                        + Path.GetExtension(input.FullName)));

            var domain = AppDomain.CreateDomain("Merging");
            
            var merger = 
              (DerivedAssemblyMerger)domain.CreateInstanceAndUnwrap(
                  typeof(DerivedAssemblyMerger).Assembly.FullName, 
                  typeof(DerivedAssemblyMerger).FullName);

            merger.Merge(input, output);

            AppDomain.Unload(domain);

            if (output.Exists)
            {
                input.Delete();
                output.MoveTo(input.FullName);
            }
        }
    }
}
