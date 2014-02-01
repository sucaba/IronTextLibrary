using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce]
        CsTypeName TypeName(CsNamespaceOrTypeName name);

        [Produce]
        CsNamespaceName NamespaceName(CsNamespaceOrTypeName name);

        [Produce]
        CsNamespaceOrTypeName NamespaceOrTypeName(
                CsIdentifier            name, 
                Opt<CsTypeArgumentList> typeArgs);

        [Produce(null, ".", null)]
        CsNamespaceOrTypeName NamespaceOrTypeName(
                CsNamespaceOrTypeName   declaring,
                CsIdentifier            identifier,
                Opt<CsTypeArgumentList> typeArgs);

        [Produce]
        CsNamespaceOrTypeName NamespaceOrTypeName(
            CsQualifiedAliasMember qualifiedAliasMember);

        [Produce]
        CsType Type(CsValueType valueType);

        [Produce]
        CsType Type(CsReferenceType referenceType);

        /* TODO: Syntactic ambiguity with other types referenced by name
        [Parse]
        CsType Type(CsTypeParameter typeParameter);
        */

        [Produce]
        CsValueType ValueType(CsStructType structType);

        [Produce]
        CsValueType ValueType(CsEnumType structType);

        [Produce]
        CsStructType StructType(CsTypeName name);

        [Produce]
        CsStructType StructType(CsSimpleType simpleType);

        [Produce]
        CsStructType StructType(CsNullableType nullable);

        [Produce]
        CsSimpleType SimpleType(CsNumericType numeric);

        [Produce("bool")]
        CsSimpleType SimpleType();

        [Produce]
        CsNumericType NumericType(CsIntegralType type);

        [Produce]
        CsNumericType NumericType(CsFloatingPointType type);

        [Produce("decimal")]
        CsNumericType NumericType();

        [Produce("sbyte")]
        [Produce("byte")]
        [Produce("short")]
        [Produce("ushort")]
        [Produce("int")]
        [Produce("uint")]
        [Produce("long")]
        [Produce("ulong")]
        [Produce("char")]
        CsIntegralType IntegralType();

        [Produce("float")]
        [Produce("double")]
        CsFloatingPointType FloatingPointType();

        [Produce(null, "?")]
        CsNullableType NullableType(CsNonNullableValueType type);

        [Produce]
        CsNonNullableValueType NonNullableValueType(CsType type);

        [Produce]
        CsEnumType EnumType(CsTypeName typeName);

        [Produce]
        CsReferenceType ReferenceType(CsClassType type);

        [Produce]
        CsReferenceType ReferenceType(CsInterfaceType type);

        [Produce]
        CsReferenceType ReferenceType(CsArrayType type);

        [Produce]
        CsReferenceType ReferenceType(CsDelegateType type);

        [Produce]
        CsClassType ClassType(CsTypeName typeName);

        [Produce("object")]
        [Produce("dynamic")]
        [Produce("string")]
        CsClassType ClassType();

        [Produce]
        CsInterfaceType InterfaceType(CsTypeName typeName);

        [Produce("[", null, "]")]
        CsRankSpecifier RankSpecifier(CsDimSeparators dim);

        [Produce]
        CsDelegateType DelegateType(CsTypeName typeName);

        [Produce("<", null, ">")]
        CsTypeArgumentList TypeArguments(CsCommaList<CsTypeArgument> typeArgs);

        [Produce]
        CsTypeArgument TypeArgument(CsType type);

        [Produce]
        CsTypeParameter TypeParameter(CsIdentifier id);
    }
}
