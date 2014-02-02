using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce(null, null, "delegate", null, null, null, "(", null, ")", null, ";")]
        CsDelegateDeclaration DelegateDeclaration(
                Opt<CsAttributes>        attributes,
                Opt<CsList<CsDelegateModifier>> modifiers,
                CsReturnType                    returnType,
                CsIdentifier                    id,
                Opt<CsVariantTypeParameterList> typeParams,
                Opt<CsFormalParameterList>      formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParameterConstraints);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        CsDelegateModifier DelegateModifier();
    }
}
