namespace AtlusLibSharp.Utilities
{
    using System;

    public static class BitHelper
    {
        // Bitwise operations extensions
        public static bool IsBitSet<T>(T value, int bitIndex) 
            where T: IConvertible
        {
            return ((Convert.ToUInt32(value) & (1 << bitIndex)) >> bitIndex) == 1;
        }

        public static void SetBit<T>(ref T value, int bitIndex)
            where T : IConvertible
        {
            value = (T)Convert.ChangeType(Convert.ToUInt32(value) | (uint)(1 << bitIndex), typeof(T));
        }

        public static void ClearBit<T>(ref T value, int bitIndex)
            where T : IConvertible
        {
            value = (T)Convert.ChangeType(Convert.ToUInt32(value) & ~(uint)(1 << bitIndex), typeof(T));
        }

        public static void ClearAndSetBits(ref uint value, int numBitsToSet, uint valueToSet, int bitStartIndex = 0)
        {
            unchecked
            {
                uint bitsToSet = FillBits(numBitsToSet);
                value &= ~bitsToSet;
                value |= (uint)(valueToSet & numBitsToSet) << bitStartIndex;
            }
        }

        public static uint GetBits(uint value, int numBitsToGet, int bitStartIndex)
        {
            uint bitsToGet = FillBits(numBitsToGet);
            return (value & (bitsToGet << bitStartIndex)) >> bitStartIndex;
        }

        private static uint FillBits(int numBits)
        {
            uint value = 0;
            for (int i = 0; i < numBits; i++)
            {
                value |= (uint)1 << i;
            }
            return value;
        }
    }
}
