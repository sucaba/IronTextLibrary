using System;
using IronText.Build;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IronText.MsBuild
{
    public class DeriveTask : AppDomainIsolatedTask
    {
        public const string ManagedDerivatorName = "Managed";
        public const string ManagedDerivatorTypeName = "IronText.Build.AssemblyDerivator, IronText.Compiler";

        public DeriveTask()
        {
            this.SourceAssemblies = new ITaskItem[0];
            this.DerivedAssemblies = new ITaskItem[0];
        }

        [Required]
        public string DerivatorNames { get; set; }

        [Required]
        public ITaskItem[] SourceAssemblies { get; set; }

        [Required]
        public ITaskItem[] DerivedAssemblies { get; set; }
        
        public override bool Execute()
        {
            if (SourceAssemblies.Length == 0)
            {
                Log.LogError("Expected one or more SourceAssemblies but got 0");
                return false;
            }

            if (SourceAssemblies.Length != DerivedAssemblies.Length)
            {
                Log.LogError(
                        "Count of SourceAssemblies and DerivedAssemblies do not match (count is {0} and {1} correspondingly).", 
                        SourceAssemblies.Length,
                        DerivedAssemblies.Length);
                return false;
            }

            Log.LogMessage(MessageImportance.Low, "Deriving from {0} source assemblies", SourceAssemblies.Length);

            bool result = true;
            var derivatorNames = DerivatorNames.Split(new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var derivatorName in derivatorNames)
                for (int i = 0; i != SourceAssemblies.Length; ++i)
                {
                    var sourcePath = SourceAssemblies[i].ItemSpec;
                    var derivedPath = DerivedAssemblies[i].ItemSpec;
                    result = ExecuteOne(sourcePath, derivedPath, derivatorName) && result;
                }

            return result;
        }

        public bool ExecuteOne(string sourcePath, string derivedPath, string derivatorName)
        {
            Log.LogMessage(MessageImportance.Low, "Deriving from {0}", sourcePath);

            try
            {
                var derived = new Derived();
                derived.Execute(
                    new MsBuildTaskLogger(this.Log), 
                    sourcePath, GetDerivatorTypeName(derivatorName), derivedPath);
                return true;
            }
            catch (Exception e)
            {
                Exception error = e;
                while (error.InnerException != null)
                {
                    error = error.InnerException;
                }

                this.Log.LogErrorFromException(error, true);
                return false;
            }
        }

        private string GetDerivatorTypeName(string derivatorName)
        {
            switch (derivatorName)
            {
                case ManagedDerivatorName: return ManagedDerivatorTypeName;
            }

            return derivatorName;
        }
    }
}
