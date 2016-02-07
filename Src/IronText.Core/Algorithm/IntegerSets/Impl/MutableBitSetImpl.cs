using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IronText.Algorithm.IntegerSets.Impl
{
    [Serializable]
    class MutableBitSetImpl : IMutableBitSet<MutableBitSetImpl>
    {
        private const int BitsInWord = 0x20;
        private uint[] words;

        internal MutableBitSetImpl(int bitCount, bool defaultBit)
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

        internal MutableBitSetImpl(uint[] words)
        {
            this.words = words;
        }

        public void Add(int value)
        {
            words[value / BitsInWord] |= (1u << (value % BitsInWord));
        }

        public void Add(IntInterval interval)
        {
            int first = interval.First;
            int last = interval.Last;
            while (first <= last)
            {
                Add(first++);
            }
        }

        public int PopAny()
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

        public void Remove(int value)
        {
            words[value / BitsInWord] &= ~(1u << (value % BitsInWord));
        }

        public void AddAll(MutableBitSetImpl other)
        {
            var otherWords = other.words;
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

        public void RemoveAll(MutableBitSetImpl other)
        {
            var otherWords = other.words;
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

        public bool Contains(int value) 
        { 
            int index = value / BitsInWord;
            if (index >= words.Length)
            {
                return false;
            }

            return 0 != (words[index] & (1 << (value % BitsInWord)));
        }

        public bool IsEmpty
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

        public int Count
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

        public MutableBitSetImpl Union(MutableBitSetImpl other)
        {
            var otherWords = other.words;
            int otherLen = otherWords.Length;
            int length = words.Length;
            int lengthDiff = length - otherLen;

            int maxLength;
            int commonLength;

            if (lengthDiff > 0)
            {
                maxLength     = length;
                commonLength  = otherLen;
            }
            else
            {
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

            return new MutableBitSetImpl(resultWords);
        }

        public MutableBitSetImpl Intersect(MutableBitSetImpl other)
        {
            var otherWords = other.words;
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
                resultWords[i] = words[i] & otherWords[i];
            }

            return new MutableBitSetImpl(resultWords);
        }

        public MutableBitSetImpl Complement(MutableBitSetImpl vocabulary)
        {
            var otherWords = vocabulary.words;
            int otherLength = otherWords.Length;
            int length = words.Length;

            uint[] resultWords = new uint[otherLength];
            for (int i = 0; i != otherLength; ++i)
            {
               resultWords[i] = otherWords[i] & ~words[i];
            }

            return new MutableBitSetImpl(resultWords);
        }

        public bool Equals(MutableBitSetImpl other)
        {
            var otherWords = other.words;
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

        public MutableBitSetImpl Clone()
        {
            var newWords = new uint[words.Length];
            words.CopyTo(newWords, 0);
            return new MutableBitSetImpl(newWords);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<int> GetEnumerator()
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

        public override bool Equals(object obj)
        {
            return Equals(obj as MutableBitSetImpl);
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
    }
}