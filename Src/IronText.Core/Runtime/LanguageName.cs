using System;
using System.IO;
using System.Reflection;

namespace IronText.Runtime
{
    /// <summary>
    /// Language identity and naming policy for language assembly
    /// </summary>
    public class LanguageName
    {
        private readonly Type definitionType;

        public LanguageName(Type moduleType)
        {
            this.definitionType = moduleType;
        }

        public Assembly SourceAssembly
        {
            get { return definitionType.Assembly; }
        }

        public string FullName 
        { 
            get { return definitionType.FullName; }
        }

        public string Name 
        { 
            get { return definitionType.Name; }
        }

        public string SourceAssemblyPath
        {
            get { return new Uri(SourceAssembly.CodeBase).LocalPath; }
        }

        public string SourceAssemblyDirectory
        {
            get { return Path.GetDirectoryName(SourceAssemblyPath); }
        }

        public string GrammarFileName
        {
            get { return LanguageTypeName + ".gram"; }
        }

        public string ScannerInfoFileName
        {
            get { return LanguageTypeName + ".scan"; }
        }

        public string GrammarInfoFileName
        {
            get { return LanguageTypeName + ".info"; }
        }

        public string LanguageTypeName
        {
            get { return definitionType.Namespace + ".Derived." + definitionType.Name + "_Language"; }
        }

        public override string ToString() { return FullName; }

        public override bool Equals(object obj)
        {
            var casted = obj as LanguageName;
            return casted != null && casted.FullName == FullName;
        }

        public override int GetHashCode() { return FullName.GetHashCode(); }

        internal Type DefinitionType { get { return definitionType; } }
    }
}
