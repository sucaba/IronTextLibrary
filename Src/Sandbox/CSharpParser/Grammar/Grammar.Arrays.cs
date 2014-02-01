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
        CsArrayType ArrayType(
                CsNonArrayType          elementType,
                CsList<CsRankSpecifier> rankSpecifiers);

        [Produce("{", null, "}")]
        CsArrayInitializer ArrayInitializer(
                Opt<CsCommaList<CsVariableInitializer>> initialziers);

        [Produce("{", null, ",", "}")]
        CsArrayInitializer ArrayInitializer(
                CsCommaList<CsVariableInitializer> initialziers);

        [Produce]
        CsNonArrayType NonArrayType(CsType type);
    }
}
