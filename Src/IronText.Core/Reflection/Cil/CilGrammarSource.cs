using System;
using System.IO;
using System.Reflection;
using IronText.Misc;

namespace IronText.Reflection.Managed
{
    /// <summary>
    /// Language identity and naming policy for language assembly
    /// </summary>
    public class CilGrammarSource : IGrammarSource
    {
        private readonly Type definitionType;

        public CilGrammarSource(Type definitionType)
        {
            this.definitionType = definitionType;
        }

        public Assembly SourceAssembly
        {
            get { return definitionType.Assembly; }
        }

        public string FullLanguageName 
        { 
            get { return definitionType.FullName; }
        }

        public string LanguageName 
        { 
            get { return definitionType.Name; }
        }

        public string Origin
        {
            get { return ReflectionUtils.ToString(definitionType); }
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

        public override string ToString() { return FullLanguageName; }

        public override bool Equals(object obj)
        {
            var casted = obj as CilGrammarSource;
            return casted != null && casted.FullLanguageName == FullLanguageName;
        }

        public override int GetHashCode() { return FullLanguageName.GetHashCode(); }

        internal Type DefinitionType { get { return definitionType; } }
    }
}
