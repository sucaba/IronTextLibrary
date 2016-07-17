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
            if (definitionType == null)
            {
                throw new ArgumentNullException(nameof(definitionType));
            }

            this.DefinitionType     = definitionType;
            this.SourceAssembly     = DefinitionType.Assembly;
            this.FullLanguageName   = DefinitionType.FullName;
            this.LanguageName       = DefinitionType.Name;
            this.GrammarOrigin      = ReflectionUtils.ToString(DefinitionType);
            this.GrammarFileName    = LanguageTypeName + ".gram";
            this.LanguageTypeName   = DefinitionType.FullName.Replace('+', '$') + "$_Language";
        }

        public Type     DefinitionType      { get; }

        public Assembly SourceAssembly      { get; }

        public string   FullLanguageName    { get; }

        public string   LanguageName        { get; }

        public string   LanguageTypeName    { get; }

        public string   GrammarOrigin       { get; }

        public string   GrammarFileName     { get; }

        public string   ScannerInfoFileName => LanguageTypeName + ".scan";

        public string   GrammarInfoFileName => LanguageTypeName + ".info";

        public override string ToString() => FullLanguageName;

        public override bool Equals(object obj)
        {
            var casted = obj as TypedLanguageSource;
            return casted != null && casted.FullLanguageName == FullLanguageName;
        }

        public override int GetHashCode() => FullLanguageName.GetHashCode();

        public string GrammarReaderTypeName =>
            "IronText.Reflection.Managed.CilGrammarReader, IronText.Compiler";

        string IReportDestinationHint.OutputDirectory => SourceAssemblyDirectory;

        private string SourceAssemblyPath =>
            new Uri(SourceAssembly.CodeBase).LocalPath;

        private string SourceAssemblyDirectory =>
            Path.GetDirectoryName(SourceAssemblyPath);
    }
}
