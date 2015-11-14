using IronText.Reflection;
using IronText.Reflection.Managed;
using System;

namespace IronText.Runtime
{
    class CilExistingLanguageLoader : ILanguageLoader
    {
        public ILanguageRuntime Load(ILanguageSource name)
        {
            var cilLanguageName = name as TypedLanguageSource;
            if (cilLanguageName == null)
            {
                return null;
            }

            var typeName = cilLanguageName.LanguageTypeName;
            string[] assemblyNameCandidates = {
                cilLanguageName.SourceAssembly.GetName().Name,
                cilLanguageName.SourceAssembly.GetName().Name + ".Derived"
            };

            foreach (var assemblyName in assemblyNameCandidates)
            {
                var fullTypeName = typeName + ", " + assemblyName;
                var type = Type.GetType(fullTypeName);
                if (type == null)
                {
                    continue;
                }

                return (ILanguageRuntime)Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
