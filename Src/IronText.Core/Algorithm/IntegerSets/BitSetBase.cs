using System;
using System.Collections.Generic;

namespace IronText.Algorithm
{
    public class BitSetBase : MutableIntSet
    {
        private const int BitsInWord = 0x20;
        private uint[] words;

        internal BitSetBase(IntSetType setType, int bitCount, bool defaultBit)
            : base(setType)
        {
            words = new uint[(bitCount - 1)/BitsInWord  + 1];
            if (defaultBit)
            {
                int len = words.Length;
                for (int i = 0; i != len; ++i)
                {
                    words[i] = ~0u;
                }
            }
        }

        internal BitSetBase(IntSetType setType, uint[] words)
            : base(setType)
        {
            this.words = words;
        }

        public override void Add(int value)
        {
            words[value / BitsInWord] |= (1u << (value % BitsInWord));
        }

        public override void Add(IntInterval interval)
        {
            int first = interval.First;
            int last = interval.Last;
            while (first <= last)
            {
                Add(first++);
            }
        }

        public override int PopAny()
        {
            int count = words.Length;
            for (int i = 0; i != count; ++i)
            {
                uint word = words[i];

                if (word == 0)
                {
                    continue;
                }

                byte bitIndex = Ntz(word);
                words[i] = word & ~(1u << bitIndex);
                return bitIndex + i * BitsInWord;
            }

            throw new InvalidOperationException("Unable to pop element from the empty set");
        }

        public override void Remove(int value)
        {
            words[value / BitsInWord] &= ~(1u << (value % BitsInWord));
        }

        public override void AddAll(IntSet other)
        {
            var casted = (BitSetBase)other;
            var otherWords = casted.words;
            int otherLength = otherWords.Length;
            int len = words.Length;
            if (len < otherLength)
            {
                throw new InvalidOperationException("Insufficient bit set size. Other set is larger");
            }

            for (int i = 0; i != otherLength; ++i)
            {
                words[i] |= otherWords[i];
            }
        }

        public override void RemoveAll(IntSet other)
        {
            var casted = (BitSetBase)other;
            var otherWords = casted.words;
            int otherLength = otherWords.Length;
            int len = words.Length;
            if (len < otherLength)
            {
                throw new InvalidOperationException("Insufficient bit set size. Other set is larger");
            }

            for (int i = 0; i != otherLength; ++i)
            {
                words[i] &= ~otherWords[i];
            }
        }

        public override IntSet CompleteAndDestroy()
        {
            var result = new BitSet(setType, words);
            this.words = null;
            return result;
        }

        public override bool Contains(int value) 
        { 
            int index = value / BitsInWord;
            if (index >= words.Length)
            {
                return false;
            }

            return 0 != (words[index] & (1 << (value % BitsInWord)));
        }

        public override bool IsEmpty
        {
            get 
            {
                for (int i = 0; i != words.Length; ++i)
                {
                    if (words[i] != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override int Count
        {
            get 
            {
                uint result = 0;
                int len = words.Length;
                for (int i = 0; i != len; ++i)
                {
                    result += CountOfBits(words[i]);
                }

                return (int)result;
            }
        }

        public override IntSet Union(IntSet other)
        {
            var casted = (BitSetBase)other;

            var otherWords = casted.words;
            int otherLen = otherWords.Length;
            int length = words.Length;
            int lengthDiff = length - otherLen;

            int maxLength;
            int commonLength;

            IntSetType resultSetType;

            if (lengthDiff > 0)
            {
                resultSetType = this.setType;
                maxLength     = length;
                commonLength  = otherLen;
            }
            else
            {
                resultSetType = casted.setType;
                maxLength     = otherLen;
                commonLength  = length;
            }

            uint[] resultWords = new uint[maxLength];
            for (int i = 0; i != commonLength; ++i)
            {
               resultWords[i] = words[i] | otherWords[i];
            }

            if (lengthDiff == 0)
            {
            }
            else if (lengthDiff > 0)
            {
                Array.Copy(words, commonLength, resultWords, commonLength, lengthDiff);
            }
            else
            {
                Array.Copy(otherWords, commonLength, resultWords, commonLength, -lengthDiff);
            }

            return new BitSet(resultSetType, resultWords);
        }

        public override IntSet Intersect(IntSet other)
        {
            var casted = (BitSetBase)other;

            var otherWords = casted.words;
            int otherLen = otherWords.Length;
            int length = words.Length;

            IntSetType resultSetType;

            int commonLength;

            if (length > otherLen)
            {
                resultSetType = casted.setType;
                commonLength = otherLen;
            }
            else
            {
                resultSetType = this.setType;
                commonLength = length;
            }

            uint[] resultWords = new uint[commonLength];
            for (int i = 0; i != commonLength; ++i)
            {
                resultWords[i] = words[i] & otherWords[i];
            }

            return new BitSet(setType, resultWords);
        }

        public override IntSet Complement(IntSet vocabulary)
        {
            var casted = (BitSetBase)vocabulary;

            var otherWords = casted.words;
            int otherLength = otherWords.Length;
            int length = words.Length;

            uint[] resultWords = new uint[otherLength];
            for (int i = 0; i != otherLength; ++i)
            {
               resultWords[i] = otherWords[i] & ~words[i];
            }

            return new BitSet(vocabulary.setType, resultWords);
        }

        public override bool SetEquals(IntSet other)
        {
            var casted = (BitSetBase)other;

            var otherWords = casted.words;
            int otherLen = otherWords.Length;
            int length = words.Length;

            int commonLength;

            if (length > otherLen)
            {
                commonLength = otherLen;
            }
            else
            {
                commonLength = length;
            }

            uint[] resultWords = new uint[commonLength];
            for (int i = 0; i != commonLength; ++i)
            {
                if (words[i] != otherWords[i])
                {
                    return false;
                }
            }

            int diff = length - otherLen;
            if (diff == 0)
            {
            }
            else if (diff > 0)
            {
                for (int i = commonLength; i != length; ++i)
                {
                    if (words[i] != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                for (int i = commonLength; i != otherLen; ++i)
                {
                    if (otherWords[i] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override IntSet Clone()
        {
            var newWords = new uint[words.Length];
            words.CopyTo(newWords, 0);
            return new BitSet(setType, newWords);
        }

        public override int Min()
        {
            throw new NotImplementedException();
        }

        public override int Max()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<int> GetEnumerator()
        {
            int len = words.Length;
            int current = 0;
            for (int wordIndex = 0; wordIndex != len; ++wordIndex)
            {
                uint word = words[wordIndex];
                if (word == 0)
                {
                    current += BitsInWord;
                }
                else
                {
                    for (int bitIndex = 0; bitIndex != BitsInWord; ++bitIndex)
                    {
                        if (0 != (word & (1 << bitIndex)))
                        {
                            yield return current;
                        }

                        ++current;
                    }
                }
            }
        }

        public override MutableIntSet EditCopy()
        {
            var newWords = new uint[words.Length];
            words.CopyTo(newWords, 0);
            return new MutableBitSet(setType, newWords);
        }

        public override IEnumerable<IntInterval> EnumerateIntervals()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (words.Length == 1)
                {
                    return (int)words[0];
                }
                else
                {
                    return (int)(words[0] ^ words[words.Length - 1]);
                }
            }
        }

        private static int GetWordIndex(int value) { return value / BitsInWord; }

        private static int GetBitIndex(int value) { return value % BitsInWord; }

        private static uint CountOfBits(uint x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            return (((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        /// <summary>
        /// Least significant zero count
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static byte Ntz(uint x)
        {
            if (x == 0)
            {
                return 32; 
            }

            uint y;
            byte n = 31;

            y = x << 16; if (y != 0) { n -= 16; x = y; }
            y = x << 8;  if (y != 0) { n -= 8; x = y; }
            y = x << 4;  if (y != 0) { n -= 4; x = y; }
            y = x << 2;  if (y != 0) { n -= 2; x = y; }
            y = x << 1;  if (y != 0) { n -= 1; x = y; }

            return n;
        }


        public override string ToCharSetString()
        {
            throw new NotImplementedException();
        }
    }
}
