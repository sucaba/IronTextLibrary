using System;

namespace IronText.Framework
{
    class ExistingLanguageLoader : ILanguageLoader
    {
        public ILanguage Load(LanguageName languageName)
        {
            var typeName = languageName.LanguageTypeName;
            string[] assemblyNameCandidates = {
                languageName.SourceAssembly.GetName().Name,
                languageName.SourceAssembly.GetName().Name + ".Derived"
            };

            foreach (var assemblyName in assemblyNameCandidates)
            {
                var fullTypeName = typeName + ", " + assemblyName;
                var type = Type.GetType(fullTypeName);
                if (type == null)
                {
                    continue;
                }

                return (ILanguage)Activator.CreateInstance(type, languageName);
            }

            return null;
        }
    }
}
