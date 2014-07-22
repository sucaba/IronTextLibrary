using System.Collections.Generic;
using System.Linq;
using IronText.Framework;
using IronText.Lib.Shared;

namespace IronText.Lib.IL
{
    public class Name1
    {
        private string[] parts;

        public Name1(params string[] parts)
        {
            this.parts = parts;
        }

        public string FullName
        {
            get { return string.Join(".", parts); }
        }

        public string Namespace
        {
            get { return string.Join(".", parts, 0, parts.Length - 1); }
        }

        public string Name
        {
            get { return parts[parts.Length - 1];  }
        }
    }

    public class SlashedName
    {
        public readonly Name1[] Parts;

        public SlashedName(params Name1[] parts)
        {
            this.Parts = parts;
        }

        public string Name
        {
            get
            {
                return string.Join("/", this.Parts.Select(n => n.FullName), "/");
            }
        }
    }

    public class ClassName
    {
        public readonly Ref<ResolutionScopes> Scope;
        public readonly SlashedName SlashedName;

        public static ClassName Parse(string name)
        {
            string[] slashedParts = name.Split('/');
            var dotted = new List<Name1>();
            foreach (var slashedPart in slashedParts)
            {
                dotted.Add(new Name1(slashedPart.Split('.')));
            }

            return new ClassName(new SlashedName(dotted.ToArray()));
        }

        public ClassName(Ref<ResolutionScopes> scope, SlashedName slashedName)
        {
            this.Scope       = scope;
            this.SlashedName = slashedName;
        }

        public ClassName(SlashedName slashedName)
            : this(null, slashedName)
        {
        }

        public string Name
        {
            get { return SlashedName.Name; }
        }
    }

    public class Bounds1
    {
    }

    public class FieldSpec
    {
        public Ref<Types> FieldType { get; set; }

        public Ref<Types> DeclType  { get; set; }

        public string     FieldName { get; set; }
    }

    public class TypeSpec
    {
        public Ref<Types> Type;

        public static implicit operator TypeSpec(Ref<Types> typeRef) { return new TypeSpec { Type = typeRef }; } 
    }

    public class Bytes
    {
        public readonly byte[] Data;

        public Bytes(byte[] data) { Data = data; }

        public static Bytes Parse(string text)
        {
            List<byte> data = new List<byte>(8);

            int length = text.Length;
            int end = length;
            for (int pos = 0; pos != end; ++pos)
            {
                if (char.IsWhiteSpace(text[pos]))
                {
                    continue;
                }

                char highHex = text[pos];
                char lowHex = text[++pos];
                data.Add(ByteFromChars(highHex, lowHex));
            }

            return new Bytes(data.ToArray());
        }

        private static byte ByteFromChars(char highHex, char lowHex)
        {
            return (byte)((Hex(highHex) << 4) + Hex(lowHex));
        }

        private static int Hex(char hexDigit)
        {
            if (hexDigit <= '9')
            {
                return hexDigit - '0';
            }

            return (char.ToLower(hexDigit) - 'a') + 10;
        }
    }

    [Vocabulary]
    [StaticContext(typeof(Builtins))]
    public static class CilPrimitives
    {
        [Match("'(' blank* (hex blank* hex blank*)+ ')'")]
        public static Bytes ByteSeq(string text)
        {
            return Bytes.Parse(text.Substring(1, text.Length - 2));
        }

        [Produce]
        public static Pipe<T1, T2> Pipe<T1, T2>(T2 value) { return _ => value; }

        [Produce]
        public static Name1 Name1(string part0) { return new Name1(part0); }

        [Produce(null, ".", null)]
        public static Name1 Name1(string part0, string part1) { return new Name1(part0, part1); }

        [Produce(null, ".", null, ".", null)]
        public static Name1 Name1(string part0, string part1, string part2) { return new Name1(part0, part1, part2); }

        [Produce]
        public static SlashedName SlashedName(Name1 name1) { return new SlashedName(name1); }

        [Produce(null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y) { return new SlashedName(x, y); } 

        [Produce(null, "/", null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y, Name1 z) { return new SlashedName(x, y, z); } 

        [Produce(null, "/", null, "/", null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y, Name1 z, Name1 t) { return new SlashedName(x, y, z, t); } 

        [Produce]
        public static ClassName ClassName(SlashedName slashedName)
        {
            return new ClassName(slashedName);
        }

        [Produce(null, null, "::", null)]
        public static FieldSpec Field(Ref<Types> fieldType, TypeSpec declaringType, string fieldName)
        {
            return new FieldSpec
            {
                FieldType = fieldType,
                DeclType  = declaringType.Type,
                FieldName = fieldName,
            };
        }
    }
}
