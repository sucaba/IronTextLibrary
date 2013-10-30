using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse]
        CsCompilationUnit CompilationUnit(
                Opt<CsList<CsExternAliasDirective>>        externAliasDirectives,
                Opt<CsList<CsUsingDirective>>              usingDirectives,
                Opt<CsGlobalAttributes>                  globalAttributes,
                Opt<CsList<CsNamespaceMemberDeclaration>>  nsMemberDeclarations);

        [Parse("namespace", null, null)]
        [Parse("namespace", null, null, ";")]
        CsNamespaceDeclaration NamespaceDeclaration(
                CsQualifiedIdentifier               name,
                CsNamespaceBody                     body);

        [Parse]
        CsQualifiedIdentifier QualifiedIdentifier(CsDotList<CsIdentifier> identifiers);

        [Parse("{", null, null, null, "}")]
        CsNamespaceBody NamespaceBody(
                Opt<CsList<CsExternAliasDirective>>        externAliasDirectives,
                Opt<CsList<CsUsingDirective>>              usingDirectives,
                Opt<CsList<CsNamespaceMemberDeclaration>>  nsMemberDeclarations);

        [Parse("extern", "alias", null)]
        CsExternAliasDirective ExternAliasDirective(
                CsIdentifier identifier);

        [Parse]
        CsUsingDirective UsingDirective(CsUsingAliasDirective directive);

        [Parse]
        CsUsingDirective UsingDirective(CsUsingNamespaceDirective directive);

        [Parse("using", null, "=", null, ";")]
        CsUsingAliasDirective UsingAliasDirective(
                CsIdentifier          identifier,
                CsNamespaceOrTypeName namespaceOrTypeName);

        [Parse("using", null, ";")]
        CsUsingNamespaceDirective UsingNamespaceDirective(
                CsNamespaceName       namespaceName);

        [Parse]
        CsNamespaceMemberDeclaration NamespaceMemberDeclaration(
                CsNamespaceDeclaration declaration);

        [Parse]
        CsNamespaceMemberDeclaration NamespaceMemberDeclaration(
                CsTypeDeclaration declaration);

        [Parse]
        CsTypeDeclaration TypeDeclaration(CsClassDeclaration decl);

        [Parse]
        CsTypeDeclaration TypeDeclaration(CsStructDeclaration decl);

        [Parse]
        CsTypeDeclaration TypeDeclaration(CsInterfaceDeclaration decl);

        [Parse]
        CsTypeDeclaration TypeDeclaration(CsEnumDeclaration decl);

        [Parse]
        CsTypeDeclaration TypeDeclaration(CsDelegateDeclaration decl);

        [Parse(null, "::", null, null)]
        CsQualifiedAliasMember QualifiedAliasMember(
                CsIdentifier            id1,
                CsIdentifier            id2,
                Opt<CsTypeArgumentList> typeArgs);
    }
}
