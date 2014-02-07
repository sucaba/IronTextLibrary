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
        public CilGrammarSource(Type definitionType)
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

        public string Origin
        {
            get { return ReflectionUtils.ToString(DefinitionType); }
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
            get { return DefinitionType.Namespace + ".Derived." + DefinitionType.Name + "_Language"; }
        }

        public override string ToString() { return FullLanguageName; }

        public override bool Equals(object obj)
        {
            var casted = obj as CilGrammarSource;
            return casted != null && casted.FullLanguageName == FullLanguageName;
        }

        public override int GetHashCode() { return FullLanguageName.GetHashCode(); }

        public string BuilderTypeName
        {
            get { return "IronText.MetadataCompiler.CilSyntax.CilGrammarBuilder, IronText.Compiler"; }
        }
    }
}
