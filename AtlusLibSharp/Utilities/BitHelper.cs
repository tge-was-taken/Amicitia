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
    }
}
