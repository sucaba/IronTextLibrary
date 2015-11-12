using IronText.Framework;

namespace IronText.Lib.IL
{
    [Demand]
    public interface WantFieldInit
        : ClassSyntax
        , WantFieldInitThen<ClassSyntax>
    {
    }

    [Demand]
    public interface WantFieldInitThen<TNext>
    {
#if false
        [Produce("float32", "(", null, ")")]
        TNext Init(float value);

        [Produce("float64", "(", null, ")")]
        TNext Init(double value);

        [Produce("float32", "(", null, ")")]
        TNext Init(long value);

        [Produce("float64", "(", null, ")")]
        TNext Init(long value);

        [Produce("int64", "(",  null, ")")]
        TNext Init(long value);

        [Produce("int32", "(", null, ")")]
        TNext Init(long value);

        [Produce("int16", "(", null, ")")]
        TNext Init(long value);

        [Produce("char", "(", null, ")")]
        TNext Init(long value);

        [Produce("int8", "(", null, ")")]
        TNext Init(long value);

        [Produce("bool", "(", null, ")")]
        TNext Init(bool value);

        [Produce]
        TNext Init(QStr str);

        [Produce("nullref")]
        TNext InitWithNullRef();
#endif

        [Produce("bytearray")]
        TNext Init(Bytes bytes);
    }
}
