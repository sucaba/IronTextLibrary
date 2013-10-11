using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse]
        CsTypeName TypeName(CsNamespaceOrTypeName name);

        [Parse]
        CsNamespaceName NamespaceName(CsNamespaceOrTypeName name);

        [Parse]
        CsNamespaceOrTypeName NamespaceOrTypeName(
                CsIdentifier            name, 
                Opt<CsTypeArgumentList> typeArgs);

        [Parse(null, ".", null)]
        CsNamespaceOrTypeName NamespaceOrTypeName(
                CsNamespaceOrTypeName   declaring,
                CsIdentifier            identifier,
                Opt<CsTypeArgumentList> typeArgs);

        [Parse]
        CsNamespaceOrTypeName NamespaceOrTypeName(
            CsQualifiedAliasMember qualifiedAliasMember);

        [Parse]
        CsType Type(CsValueType valueType);

        [Parse]
        CsType Type(CsReferenceType referenceType);

        /* TODO: Syntactic ambiguity with other types referenced by name
        [Parse]
        CsType Type(CsTypeParameter typeParameter);
        */

        [Parse]
        CsValueType ValueType(CsStructType structType);

        [Parse]
        CsValueType ValueType(CsEnumType structType);

        [Parse]
        CsStructType StructType(CsTypeName name);

        [Parse]
        CsStructType StructType(CsSimpleType simpleType);

        [Parse]
        CsStructType StructType(CsNullableType nullable);

        [Parse]
        CsSimpleType SimpleType(CsNumericType numeric);

        [Parse("bool")]
        CsSimpleType SimpleType();

        [Parse]
        CsNumericType NumericType(CsIntegralType type);

        [Parse]
        CsNumericType NumericType(CsFloatingPointType type);

        [Parse("decimal")]
        CsNumericType NumericType();

        [Parse("sbyte")]
        [Parse("byte")]
        [Parse("short")]
        [Parse("ushort")]
        [Parse("int")]
        [Parse("uint")]
        [Parse("long")]
        [Parse("ulong")]
        [Parse("char")]
        CsIntegralType IntegralType();

        [Parse("float")]
        [Parse("double")]
        CsFloatingPointType FloatingPointType();

        [Parse(null, "?")]
        CsNullableType NullableType(CsNonNullableValueType type);

        [Parse]
        CsNonNullableValueType NonNullableValueType(CsType type);

        [Parse]
        CsEnumType EnumType(CsTypeName typeName);

        [Parse]
        CsReferenceType ReferenceType(CsClassType type);

        [Parse]
        CsReferenceType ReferenceType(CsInterfaceType type);

        [Parse]
        CsReferenceType ReferenceType(CsArrayType type);

        [Parse]
        CsReferenceType ReferenceType(CsDelegateType type);

        [Parse]
        CsClassType ClassType(CsTypeName typeName);

        [Parse("object")]
        [Parse("dynamic")]
        [Parse("string")]
        CsClassType ClassType();

        [Parse]
        CsInterfaceType InterfaceType(CsTypeName typeName);

        [Parse("[", null, "]")]
        CsRankSpecifier RankSpecifier(CsDimSeparators dim);

        [Parse]
        CsDelegateType DelegateType(CsTypeName typeName);

        [Parse("<", null, ">")]
        CsTypeArgumentList TypeArguments(CsCommaList<CsTypeArgument> typeArgs);

        [Parse]
        CsTypeArgument TypeArgument(CsType type);

        [Parse]
        CsTypeParameter TypeParameter(CsIdentifier id);
    }
}
