using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse(null, null, "delegate", null, null, null, "(", null, ")", null, ";")]
        CsDelegateDeclaration DelegateDeclaration(
                Opt<CsAttributes>        attributes,
                Opt<CsList<CsDelegateModifier>> modifiers,
                CsReturnType                    returnType,
                CsIdentifier                    id,
                Opt<CsVariantTypeParameterList> typeParams,
                Opt<CsFormalParameterList>      formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        CsDelegateModifier DelegateModifier();
    }
}
