using System;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public interface ITypeNs
    {
        Ref<Types> Import(Type type);

        [Parse]
        ClassName ClassNameInScope(Ref<ResolutionScopes> resolutionScope, SlashedName slashedName);

        [Parse]
        TypeSpec TypeSpec(ClassName className);

        [Parse]
        TypeSpec TypeSpec(Ref<Types> type);

        [Parse("class", null)]
        Ref<Types> Class_(ClassName className);

        [ParseGet("object")]
        Ref<Types> Object { get; }

        [ParseGet("string")]
        Ref<Types> String { get; }

        [Parse("valuetype", null)]
        [Parse("value", "class", null)]
        Ref<Types> Value(ClassName className);

        [Parse(null, "[", "]")]
        Ref<Types> Array(Ref<Types> elementType);

        // TODO: array bounds support
        //[Parse(null, "[", null, "]")]
        Ref<Types> Array(Ref<Types> elementType, Bounds1 bounds);

        [Parse(null, "&")]
        Ref<Types> Reference(Ref<Types> elementType);

        [Parse(null, "*")]
        Ref<Types> Pointer(Ref<Types> elementType);

        [Parse(null, "pinned")]
        Ref<Types> Pinned(Ref<Types> elementType);

        [Parse(null, "modreq", "(", null, ")")]
        Ref<Types> RequiredModifier(Ref<Types> elementType, ClassName className);

        [Parse(null, "modopt", "(", null, ")")]
        Ref<Types> OptionalModifier(Ref<Types> elementType, ClassName className);

        [Parse("!")]
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
