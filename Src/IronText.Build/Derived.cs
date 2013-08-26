using System;
using System.IO;
using System.Reflection;

namespace IronText.Build
{
    public class Derived
    {
        public void Execute(ILogger logger, string sourceAssemblyPath, string derivatorClassName, string derivedPath)
        {
            var sourceAssembly = Assembly.LoadFrom(sourceAssemblyPath);


            ResolveAssemblyByDir(
                ()=> 
                {
                    Type derivatorType = Type.GetType(derivatorClassName);
                    if (derivatorType == null)
                    {
                        logger.LogError("Unable to load derivator type from {0}", derivatorClassName);
                        return;
                    }

                    IDerivator derivator = (IDerivator)Activator.CreateInstance(derivatorType);
                    derivator.Execute(logger, sourceAssembly, derivedPath);
                },
                Path.GetDirectoryName(sourceAssemblyPath));
        }

        private void ResolveAssemblyByDir(Action action, string dir)
        {
            ResolveEventHandler resolve =
                (sender, args) =>
                {
                    var fileName = args.Name.Split(new [] {','}, 2)[0];
                    var path = Path.Combine(dir, fileName + ".dll");
                    if (File.Exists(path))
                    {
                        return Assembly.LoadFrom(path);
                    }

                    return null;
                };

            AppDomain.CurrentDomain.AssemblyResolve += resolve;

            try
            {
                action();
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolve;
            }
        }
    }
}
