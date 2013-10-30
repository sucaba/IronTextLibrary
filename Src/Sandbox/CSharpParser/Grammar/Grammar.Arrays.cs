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
        CsArrayType ArrayType(
                CsNonArrayType          elementType,
                CsList<CsRankSpecifier> rankSpecifiers);

        [Parse("{", null, "}")]
        CsArrayInitializer ArrayInitializer(
                Opt<CsCommaList<CsVariableInitializer>> initialziers);

        [Parse("{", null, ",", "}")]
        CsArrayInitializer ArrayInitializer(
                CsCommaList<CsVariableInitializer> initialziers);

        [Parse]
        CsNonArrayType NonArrayType(CsType type);
    }
}
