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

    public class TypeSpec
    {
        public Ref<Types> Type;

        public static implicit operator TypeSpec(Ref<Types> typeRef) { return new TypeSpec { Type = typeRef }; } 
    }

    public class Bytes
    {
        public readonly byte[] Data;

        public Bytes(byte[] data) { Data = data; }

        public static Bytes FromText(char[] buf, int start, int length)
        {
            List<byte> data = new List<byte>(8);
            int end = start + length;
            for (int pos = start; pos != end; ++pos)
            {
                if (char.IsWhiteSpace(buf[pos]))
                {
                    continue;
                }

                char highHex = buf[pos];
                char lowHex = buf[++pos];
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
        [Scan("'(' blank* (hex blank* hex blank*)+ ')'")]
        public static Bytes ByteSeq(char[] buf, int start, int length)
        {
            return Bytes.FromText(buf, start + 1, length - 2);
        }

        [Parse]
        public static Pipe<T1, T2> Pipe<T1, T2>(T2 value) { return _ => value; }

        [Parse]
        public static Name1 Name1(string part0) { return new Name1(part0); }

        [Parse(null, ".", null)]
        public static Name1 Name1(string part0, string part1) { return new Name1(part0, part1); }

        [Parse(null, ".", null, ".", null)]
        public static Name1 Name1(string part0, string part1, string part2) { return new Name1(part0, part1, part2); }

        [Parse]
        public static SlashedName SlashedName(Name1 name1) { return new SlashedName(name1); }

        [Parse(null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y) { return new SlashedName(x, y); } 

        [Parse(null, "/", null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y, Name1 z) { return new SlashedName(x, y, z); } 

        [Parse(null, "/", null, "/", null, "/", null)]
        public static SlashedName SlashedName(Name1 x, Name1 y, Name1 z, Name1 t) { return new SlashedName(x, y, z, t); } 

        [Parse]
        public static ClassName ClassName(SlashedName slashedName)
        {
            return new ClassName(slashedName);
        }
    }
}
