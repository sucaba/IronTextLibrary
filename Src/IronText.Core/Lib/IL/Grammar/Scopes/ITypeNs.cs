using IronText.Framework;
using IronText.Lib.Shared;
using System;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public interface ITypeNs
    {
        Ref<Types> Import(Type type);

        [Produce]
        ClassName ClassNameInScope(Ref<ResolutionScopes> resolutionScope, SlashedName slashedName);

        [Produce]
        TypeSpec TypeSpec(ClassName className);

        [Produce]
        TypeSpec TypeSpec(Ref<Types> type);

        [Produce("class", null)]
        Ref<Types> Class_(ClassName className);

        [ParseGet("object")]
        Ref<Types> Object { get; }

        [ParseGet("string")]
        Ref<Types> String { get; }

        [Produce("valuetype", null)]
        [Produce("value", "class", null)]
        Ref<Types> Value(ClassName className);

        Ref<Types> ValueType { get; }

        [Produce(null, "[", "]")]
        Ref<Types> Array(Ref<Types> elementType);

        // TODO: array bounds support
        //[Parse(null, "[", null, "]")]
        Ref<Types> Array(Ref<Types> elementType, Bounds1 bounds);

        [Produce(null, "&")]
        Ref<Types> Reference(Ref<Types> elementType);

        [Produce(null, "*")]
        Ref<Types> Pointer(Ref<Types> elementType);

        [Produce(null, "pinned")]
        Ref<Types> Pinned(Ref<Types> elementType);

        [Produce(null, "modreq", "(", null, ")")]
        Ref<Types> RequiredModifier(Ref<Types> elementType, ClassName className);

        [Produce(null, "modopt", "(", null, ")")]
        Ref<Types> OptionalModifier(Ref<Types> elementType, ClassName className);

        [Produce("!")]
        Ref<Types> GenericArg(int genericArgIndex);

        // TODO: method pointer type
        // [Parse("method")]
        // Ref<Types> MethodSig(CallConv callConv, Ref<Types> returnType, );

        [ParseGet("typedref")]
        Ref<Types> Typedref { get; }

        [ParseGet("char")]
        Ref<Types> Char { get; }

        [ParseGet("void")]
        Ref<Types> Void { get; }

        [ParseGet("bool")]
        Ref<Types> Bool { get; }

        [ParseGet("int8")]
        Ref<Types> Int8 { get; }

        [ParseGet("int16")]
        Ref<Types> Int16 { get; }

        [ParseGet("int32")]
        Ref<Types> Int32 { get; }

        [ParseGet("int64")]
        Ref<Types> Int64 { get; }

        [ParseGet("float32")]
        Ref<Types> Float32 { get; }

        [ParseGet("float64")]
        Ref<Types> Float64 { get; }

        [ParseGet("unsigned", "int8")]
        Ref<Types> UnsignedInt8 { get; }

        [ParseGet("unsigned", "int16")]
        Ref<Types> UnsignedInt16 { get; }

        [ParseGet("unsigned", "int32")]
        Ref<Types> UnsignedInt32 { get; }

        [ParseGet("unsigned", "int64")]
        Ref<Types> UnsignedInt64 { get; }

        [ParseGet("native", "int")]
        Ref<Types> NativeInt { get; }

        [ParseGet("native", "unsigned", "int")]
        Ref<Types> NativeUnsignedInt { get; }

        [ParseGet("native", "float")]
        Ref<Types> NativeFloat { get; }
    }
}
