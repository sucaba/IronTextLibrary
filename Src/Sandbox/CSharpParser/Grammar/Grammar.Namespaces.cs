using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce]
        CsCompilationUnit CompilationUnit(
                Opt<CsList<CsExternAliasDirective>>        externAliasDirectives,
                Opt<CsList<CsUsingDirective>>              usingDirectives,
                Opt<CsGlobalAttributes>                    globalAttributes,
                Opt<CsList<CsNamespaceMemberDeclaration>>  nsMemberDeclarations);

        [Produce("namespace", null, null)]
        [Produce("namespace", null, null, ";")]
        CsNamespaceDeclaration NamespaceDeclaration(
                CsQualifiedIdentifier               name,
                CsNamespaceBody                     body);

        [Produce]
        CsQualifiedIdentifier QualifiedIdentifier(CsDotList<CsIdentifier> identifiers);

        [Produce("{", null, null, null, "}")]
        CsNamespaceBody NamespaceBody(
                Opt<CsList<CsExternAliasDirective>>        externAliasDirectives,
                Opt<CsList<CsUsingDirective>>              usingDirectives,
                Opt<CsList<CsNamespaceMemberDeclaration>>  nsMemberDeclarations);

        [Produce("extern", "alias", null)]
        CsExternAliasDirective ExternAliasDirective(
                CsIdentifier identifier);

        [Produce]
        CsUsingDirective UsingDirective(CsUsingAliasDirective directive);

        [Produce]
        CsUsingDirective UsingDirective(CsUsingNamespaceDirective directive);

        [Produce("using", null, "=", null, ";")]
        CsUsingAliasDirective UsingAliasDirective(
                CsIdentifier          identifier,
                CsNamespaceOrTypeName namespaceOrTypeName);

        [Produce("using", null, ";")]
        CsUsingNamespaceDirective UsingNamespaceDirective(
                CsNamespaceName       namespaceName);

        [Produce]
        CsNamespaceMemberDeclaration NamespaceMemberDeclaration(
                CsNamespaceDeclaration declaration);

        [Produce]
        CsNamespaceMemberDeclaration NamespaceMemberDeclaration(
                CsTypeDeclaration declaration);

        [Produce]
        CsTypeDeclaration TypeDeclaration(CsClassDeclaration decl);

        [Produce]
        CsTypeDeclaration TypeDeclaration(CsStructDeclaration decl);

        [Produce]
        CsTypeDeclaration TypeDeclaration(CsInterfaceDeclaration decl);

        [Produce]
        CsTypeDeclaration TypeDeclaration(CsEnumDeclaration decl);

        [Produce]
        CsTypeDeclaration TypeDeclaration(CsDelegateDeclaration decl);

        [Produce(null, "::", null, null)]
        CsQualifiedAliasMember QualifiedAliasMember(
                CsIdentifier            id1,
                CsIdentifier            id2,
                Opt<CsTypeArgumentList> typeArgs);
    }
}
