using IronText.Misc;
using System;
using System.IO;
using System.Reflection;

namespace IronText.Runtime
{
    /// <summary>
    /// Language identity and naming policy for language assembly
    /// </summary>
    public class TypedLanguageSource : ILanguageSource, IReportDestinationHint
    {
        public TypedLanguageSource(Type definitionType)
        {
            this.DefinitionType = definitionType;
        }

        public Type DefinitionType { get; private set; }

        public Assembly SourceAssembly
        {
            get { return DefinitionType.Assembly; }
        }

        public string FullLanguageName 
        { 
            get { return DefinitionType.FullName; }
        }

        public string LanguageName 
        { 
            get { return DefinitionType.Name; }
        }

        public string GrammarOrigin
        {
            get { return ReflectionUtils.ToString(DefinitionType); }
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
            get { return DefinitionType.Namespace + ".Derived." + DefinitionType.Name + "_Language"; }
        }

        public override string ToString() { return FullLanguageName; }

        public override bool Equals(object obj)
        {
            var casted = obj as TypedLanguageSource;
            return casted != null && casted.FullLanguageName == FullLanguageName;
        }

        public override int GetHashCode() { return FullLanguageName.GetHashCode(); }

        public string GrammarReaderTypeName
        {
            get { return "IronText.Reflection.Managed.CilGrammarReader, IronText.Compiler"; }
        }

        string IReportDestinationHint.OutputDirectory
        {
            get { return SourceAssemblyDirectory; }
        }

        private string SourceAssemblyPath
        {
            get { return new Uri(SourceAssembly.CodeBase).LocalPath; }
        }

        private string SourceAssemblyDirectory
        {
            get { return Path.GetDirectoryName(SourceAssemblyPath); }
        }
    }
}
