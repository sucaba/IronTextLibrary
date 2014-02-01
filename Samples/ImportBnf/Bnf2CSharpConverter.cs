using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronText.Framework;
using IronText.Lib.Ctem;
using IronText.Runtime;
using Microsoft.CSharp;

namespace Samples
{
    class Bnf2CSharpConverter
        : BnfLanguage
        , WantDocument
        , WantRule
        , WantRuleAlternative
        , WantBranch
    {
        private string currentRuleName;
        private readonly List<object> currentAtoms = new List<object>();
        private readonly List<string> pendingComments = new List<string>();

        private readonly string sourceFile;
        private CodeCompileUnit compileUnit;
        private CodeNamespace ns;
        private CodeTypeDeclaration languageDefType;

        public static BnfLanguage Create(string baseName)
        {
            return new Bnf2CSharpConverter(baseName);
        }

        private Bnf2CSharpConverter(string baseName)
        {
            this.Scanner = new CtemScanner();

            this.sourceFile = baseName + ".cs";
            this.compileUnit = new CodeCompileUnit();

            this.ns = new CodeNamespace();
            ns.Imports.Add(new CodeNamespaceImport(typeof(Language).Namespace));
            compileUnit.Namespaces.Add(ns);

            this.languageDefType = new CodeTypeDeclaration(baseName) { IsInterface = true };
            var languageAttribute = CreateCodeAttribute<LanguageAttribute>();
            languageDefType.CustomAttributes.Add(languageAttribute);
            ns.Types.Add(languageDefType);
        }

        public CtemScanner Scanner { get; private set; }
        
        public WantDocument BeginDocument() { return this; }

        public void EndDocument()
        {
            var provider = new CSharpCodeProvider();

            using (StreamWriter sw = new StreamWriter(sourceFile, false))
            {
                IndentedTextWriter tw = new IndentedTextWriter(sw, "    ");

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(
                    compileUnit,
                    tw,
                    new CodeGeneratorOptions
                    {
                        BracingStyle = "C",
                        VerbatimOrder = true
                    });

                // Close the output file.
                tw.Close();
            }
        }

        public WantRule BeginRule(string tokenName)
        {
            currentRuleName = tokenName;
            return this;
        }

        public WantDocument EndRule() 
        {
            currentRuleName = null;
            return this; 
        }

        public WantBranch Branch() { return this; }

        public WantRuleAlternative EndBranch()
        {
            var method = new CodeMemberMethod() { Name = ToUpperCamelCase(currentRuleName) };
            method.ReturnType = GetTokenType(currentRuleName, true);

            var literalMask = new List<string>();

            foreach (var atom in currentAtoms)
            {
                var asTokenName = atom as string;
                if (asTokenName == null)
                {
                    var literal = ((QStr)atom).Text;
                    {
                        string identifier = LiteralToIdentifer(literal);
                        if (identifier.Length != 0)
                        {
                            method.Name = method.Name + "_" + ToUpperCamelCase(identifier);
                        }
                    }

                    literalMask.Add(literal);
                }
                else
                {
                    literalMask.Add(null);
                    AddArgument(method, asTokenName);
                }
            }

            // Remove trailing nulls
            int index = literalMask.FindLastIndex(literal => literal != null);
            literalMask.RemoveRange(index + 1, literalMask.Count - index - 1);

            var parseAttribute = CreateCodeAttribute<ProduceAttribute>();
            foreach (var literal in literalMask)
            {
                parseAttribute.Arguments.Add(
                    new CodeAttributeArgument(new CodePrimitiveExpression(literal)));
            }

            AddProduceMethodOrAttribute(method, parseAttribute);
            currentAtoms.Clear();
            return this; 
        }

        private static string LiteralToIdentifer(string literal)
        {
            var result = new StringBuilder();
            int i = 0;
            while (i != literal.Length && IsNotIdentifierStart(literal[i]))
            {
                ++i;
            }

            do
            {
                while (i != literal.Length && IsIdentifierMiddle(literal[i]))
                {
                    result.Append(literal[i]);
                    ++i;
                }

                while (i != literal.Length && IsIdentifierInnerDelimiter(literal[i]))
                {
                    result.Append("_");
                    ++i;
                }
            }
            while (i != literal.Length && IsIdentifierMiddle(literal[i]));

            return result.ToString();
        }

        private static bool IsIdentifierInnerDelimiter(char ch)
        {
            return ch == '.' || ch == '-';
        }

        private static bool IsIdentifierMiddle(char ch)
        {
            return ch == '_' || char.IsLetterOrDigit(ch);
        }

        private static bool IsNotIdentifierStart(char ch)
        {
            return !IsIdentifierStart(ch);
        }

        private static bool IsIdentifierStart(char ch)
        {
            return ch == '_' || char.IsLetter(ch);
        }

        private void AddArgument(CodeMemberMethod method, string tokenName)
        {
            var parameters = method.Parameters.OfType<CodeParameterDeclarationExpression>();

            string baseArgName = "arg" + method.Parameters.Count;

            string argName = baseArgName;
            var argType = GetTokenType(tokenName);

            int counter = 1;
            while (parameters.Any(p => p.Name == argName))
            {
                argName = baseArgName + ++counter;
            }
            
            method.Parameters.Add(new CodeParameterDeclarationExpression(argType, argName));
        }

        private void AddProduceMethodOrAttribute(CodeMemberMethod method, CodeAttributeDeclaration parseAttribute)
        {
            var existingMethod =
                languageDefType
                    .Members
                    .OfType<CodeMemberMethod>()
                    .Where(m => HaveSameSignature(method, m))
                    .FirstOrDefault();
            if (existingMethod != null)
            {
                existingMethod.CustomAttributes.Add(parseAttribute);
                method = existingMethod;
            }
            else
            {
                method.CustomAttributes.Add(parseAttribute);
                languageDefType.Members.Add(method);
            }

            foreach (var comment in pendingComments)
            {
                method.Comments.Add(new CodeCommentStatement(comment, true));
            }

            pendingComments.Clear();
        }

        private static bool HaveSameSignature(CodeMemberMethod x, CodeMemberMethod y)
        {
            return x.Name == y.Name
              && object.Equals(y.ReturnType.BaseType, x.ReturnType.BaseType)
              && Enumerable.SequenceEqual(
                    y.Parameters.OfType<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType),
                    x.Parameters.OfType<CodeParameterDeclarationExpression>().Select(p => p.Type.BaseType));
        }

        private CodeTypeReference GetTokenType(string tokenName, bool createIfMissing = false)
        {
            return GetTokenType(tokenName, createIfMissing, false);
        }

        private CodeTypeReference GetTermTokenType(string tokenName, bool createIfMissing = false)
        {
            return GetTokenType(tokenName, createIfMissing, true);
        }

        private CodeTypeReference GetTokenType(string tokenName, bool createIfMissing, bool isTerm)
        {
            if (string.Compare(tokenName, "void", true) == 0)
            {
                return new CodeTypeReference(typeof(void));
            }

            var typeName = ToUpperCamelCase(tokenName);
            if (null != Type.GetType("System." + typeName))
            {
                typeName = RenameExisting(typeName);
            }

            if (createIfMissing)
            {
                var tokenType = ns.Types.OfType<CodeTypeDeclaration>().FirstOrDefault(type => type.Name == typeName);
                if (tokenType == null)
                {
                    ns.Types.Add(new CodeTypeDeclaration(typeName) { IsInterface = !isTerm, IsPartial = true });
                }
            }

            var result = new CodeTypeReference(typeName);
            return result;
        }

        private string RenameExisting(string typeName)
        {
            return "@" + typeName;
        }

        public WantBranch Token(string tokenName)
        {
            currentAtoms.Add(tokenName);
            return this;
        }

        public WantBranch Literal(QStr literal)
        {
            currentAtoms.Add(literal);
            return this;
        }

        public QStr QuotedString(char[] buffer, int start, int length)
        {
            return QStr.Parse(buffer, start, length); 
        }

        public void Comment(string text) 
        {
            var comment = text.Substring(2, text.Length - 4);

            // Ignore comments containing empty.
            // They are traditionally used for marking empty rules
            if (string.Compare(comment.Trim(), "empty", true) == 0)
            {
                return;
            }

            var lines = comment.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var formattedComment = string.Join(
                                    Environment.NewLine,
                                    lines.Select(line => line.TrimStart('*', ' ', '\t')))
                                   .Trim();

            if (currentRuleName == null)
            {
                this.languageDefType.Comments.Add(new CodeCommentStatement(formattedComment, true));
            }
            else
            {
                pendingComments.Add(formattedComment);
            }
        }

        public void Blank() { }

        public void NewLine() { }

        private static string ToUpperCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name", name);
            }

            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name", name);
            }

            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private static CodeAttributeDeclaration CreateCodeAttribute<T>() where T : Attribute
        {
            return new CodeAttributeDeclaration(AttributeTypeName<T>());
        }

        private static string AttributeTypeName<T>() where T : Attribute
        {
            var result = typeof(T).Name;
            result = result.Substring(0, result.Length - "Attribute".Length);
            return result;
        }
    }
}
