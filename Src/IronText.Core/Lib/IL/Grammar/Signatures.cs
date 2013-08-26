using IronText.Framework;
using IronText.Lib.Ctem;

namespace IronText.Lib.IL
{
    [Vocabulary]
    public static class Signatures
    {
//        [Parse]
        public static TypeSig ParseTypeSig(string typeName) { return TypeSig.Parse(typeName); }

//        [Parse]
        public static TypeSig ParseTypeSig(QStr typeName) { return TypeSig.Parse(typeName.Text); }

//        [Parse]
        public static MethodSig ParseMethodSig(string typeName) { return MethodSig.Parse(typeName); }

 //       [Parse]
        public static MethodSig ParseMethodSig(QStr typeName) { return MethodSig.Parse(typeName.Text); }
    }
}
