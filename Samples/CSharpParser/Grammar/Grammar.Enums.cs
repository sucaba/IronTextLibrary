using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse(null, null, "enum", null, null, null)]
        CsEnumDeclaration EnumDeclaration(
                Opt<CsList<CsAttribute>>     attributes,
                Opt<CsList<CsEnumModifier>>  modifiers,
                CsIdentifier                 id,
                Opt<CsEnumBase>              enumBase,
                CsEnumBody                   body,
                Opt<CsSemi>                  semi);

        [Parse(":", null)]
        CsEnumBase EnumBase(CsIntegralType type);

        [Parse("{", null, "}")]
        CsEnumBody EnumBody(
                Opt<CsCommaList<CsEnumMemberDeclaration>> declarations);

        [Parse("{", null, ",", "}")]
        CsEnumBody EnumBody(
                CsCommaList<CsEnumMemberDeclaration> declarations);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        CsEnumModifier EnumModifier();

        [Parse]
        CsEnumMemberDeclaration EnumMemberDeclaration(
                Opt<CsList<CsAttribute>>     attributes,
                CsIdentifier                 id);

        [Parse(null, null, "=", null)]
        CsEnumMemberDeclaration EnumMemberDeclaration(
                Opt<CsList<CsAttribute>>     attributes,
                CsIdentifier                 id,
                CsConstantExpression         constantExpression);
    }
}
