namespace AtlusLibSharp.Common.Utilities
{
    /// <summary>
    /// Provides convienence methods for bitwise operations.
    /// </summary>
    public static class BitHelper
    {
        /// <summary>
        /// Returns a boolean value indicating if the specified bit is set.
        /// </summary>
        /// <param name="value">The value containing the bit to check.</param>
        /// <param name="bitIndex">The index of the bit to check in the value.</param>
        /// <returns><see cref="System.Boolean"/> value indicating if the specified bit is set.</returns>
        public static bool IsBitSet(ulong value, int bitIndex)
        {
            return ((value & (1u << bitIndex)) >> bitIndex) == 1;
        }

        /// <summary>
        /// Returns a boolean value indicating if the specified bit is set.
        /// </summary>
        /// <param name="value">The value containing the bit to check.</param>
        /// <param name="bitIndex">The index of the bit to check in the value.</param>
        /// <returns><see cref="System.Boolean"/> value indicating if the specified bit is set.</returns>
        public static bool IsBitSet(uint value, int bitIndex)
        {
            return ((value & (1u << bitIndex)) >> bitIndex) == 1;
        }

        /// <summary>
        /// Returns a boolean value indicating if the specified bit is set.
        /// </summary>
        /// <param name="value">The value containing the bit to check.</param>
        /// <param name="bitIndex">The index of the bit to check in the value.</param>
        /// <returns><see cref="System.Boolean"/> value indicating if the specified bit is set.</returns>
        public static bool IsBitSet(ushort value, int bitIndex)
        {
            return ((value & (1u << bitIndex)) >> bitIndex) == 1;
        }

        /// <summary>
        /// Returns a boolean value indicating if the specified bit is set.
        /// </summary>
        /// <param name="value">The value containing the bit to check.</param>
        /// <param name="bitIndex">The index of the bit to check in the value.</param>
        /// <returns><see cref="System.Boolean"/> value indicating if the specified bit is set.</returns>
        public static bool IsBitSet(byte value, int bitIndex)
        {
            return ((value & (1u << bitIndex)) >> bitIndex) == 1;
        }

        /// <summary>
        /// Sets a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to set.</param>
        /// <param name="bitIndex">The index of the bit to set in the value.</param>
        public static void SetBit(ref ulong value, int bitIndex)
        {
            value |= (1u << bitIndex);
        }

        /// <summary>
        /// Sets a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to set.</param>
        /// <param name="bitIndex">The index of the bit to set in the value.</param>
        public static void SetBit(ref uint value, int bitIndex)
        {
            value |= (1u << bitIndex);
        }

        /// <summary>
        /// Sets a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to set.</param>
        /// <param name="bitIndex">The index of the bit to set in the value.</param>
        public static void SetBit(ref ushort value, int bitIndex)
        {
            value |= (ushort)(1u << bitIndex);
        }

        /// <summary>
        /// Sets a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to set.</param>
        /// <param name="bitIndex">The index of the bit to set in the value.</param>
        public static void SetBit(ref byte value, int bitIndex)
        {
            value |= (byte)(1u << bitIndex);
        }

        /// <summary>
        /// Clear a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to clear.</param>
        /// <param name="bitIndex">The index of the bit to clear in the value.</param>
        public static void ClearBit(ref ulong value, int bitIndex)
        {
            value &= ~(1u << bitIndex);
        }

        /// <summary>
        /// Clear a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to clear.</param>
        /// <param name="bitIndex">The index of the bit to clear in the value.</param>
        public static void ClearBit(ref uint value, int bitIndex)
        {
            value &= ~(1u << bitIndex);
        }

        /// <summary>
        /// Clear a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to clear.</param>
        /// <param name="bitIndex">The index of the bit to clear in the value.</param>
        public static void ClearBit(ref ushort value, int bitIndex)
        {
            value &= (ushort)~(1u << bitIndex);
        }

        /// <summary>
        /// Clear a specified bit in the provided value.
        /// </summary>
        /// <param name="value">The value containing the bit to clear.</param>
        /// <param name="bitIndex">The index of the bit to clear in the value.</param>
        public static void ClearBit(ref byte value, int bitIndex)
        {
            value &= (byte)~(1u << bitIndex);
        }

        /// <summary>
        /// Clears the specified number of bits in the value and sets the specified value to set's bits starting from the given starting index."/>
        /// </summary>
        /// <param name="value">The value whose bits to modify.</param>
        /// <param name="numBitsToSet">The number of bits to clear and set.</param>
        /// <param name="valueToSet">The value to set in the value to modify.</param>
        /// <param name="bitStartIndex">The index of where the valueToSet's bits should begin to be set in the value to modify.</param>
        public static void ClearAndSetBits(ref ulong value, int numBitsToSet, ulong valueToSet, int bitStartIndex)
        {
            ulong bitsToSet = FillBits(numBitsToSet);
            value &= ~(bitsToSet << bitStartIndex);
            value |= (valueToSet & bitsToSet) << bitStartIndex;
        }

        /// <summary>
        /// Clears the specified number of bits in the value and sets the specified value to set's bits starting from the given starting index."/>
        /// </summary>
        /// <param name="value">The value whose bits to modify.</param>
        /// <param name="numBitsToSet">The number of bits to clear and set.</param>
        /// <param name="valueToSet">The value to set in the value to modify.</param>
        /// <param name="bitStartIndex">The index of where the valueToSet's bits should begin to be set in the value to modify.</param>
        public static void ClearAndSetBits(ref uint value, int numBitsToSet, uint valueToSet, int bitStartIndex)
        {
            uint bitsToSet = (uint)FillBits(numBitsToSet);
            value &= ~(bitsToSet << bitStartIndex);
            value |= (valueToSet & bitsToSet) << bitStartIndex;
        }

        /// <summary>
        /// Clears the specified number of bits in the value and sets the specified value to set's bits starting from the given starting index."/>
        /// </summary>
        /// <param name="value">The value whose bits to modify.</param>
        /// <param name="numBitsToSet">The number of bits to clear and set.</param>
        /// <param name="valueToSet">The value to set in the value to modify.</param>
        /// <param name="bitStartIndex">The index of where the valueToSet's bits should begin to be set in the value to modify.</param>
        public static void ClearAndSetBits(ref ushort value, int numBitsToSet, uint valueToSet, int bitStartIndex)
        {
            ushort bitsToSet = (ushort)FillBits(numBitsToSet);
            value &= (ushort)~(bitsToSet << bitStartIndex);
            value |= (ushort)((valueToSet & bitsToSet) << bitStartIndex);
        }

        /// <summary>
        /// Clears the specified number of bits in the value and sets the specified value to set's bits starting from the given starting index."/>
        /// </summary>
        /// <param name="value">The value whose bits to modify.</param>
        /// <param name="numBitsToSet">The number of bits to clear and set.</param>
        /// <param name="valueToSet">The value to set in the value to modify.</param>
        /// <param name="bitStartIndex">The index of where the valueToSet's bits should begin to be set in the value to modify.</param>
        public static void ClearAndSetBits(ref byte value, int numBitsToSet, uint valueToSet, int bitStartIndex)
        {
            byte bitsToSet = (byte)FillBits(numBitsToSet);
            value &= (byte)~(bitsToSet << bitStartIndex);
            value |= (byte)((valueToSet & bitsToSet) << bitStartIndex);
        }

        /// <summary>
        /// Returns a sequence of bits in a value using the number of bits to get and the index of where to start retrieving the bits.
        /// </summary>
        /// <param name="value">The value whose bits to get.</param>
        /// <param name="numBitsToGet">The number of bits to get in the value.</param>
        /// <param name="bitStartIndex">The index of where to start retrieving the bits in the value.</param>
        /// <returns></returns>
        public static ulong GetBits(ulong value, int numBitsToGet, int bitStartIndex)
        {
            ulong bitsToGet = FillBits(numBitsToGet);
            return (value & (bitsToGet << bitStartIndex)) >> bitStartIndex;
        }

        /// <summary>
        /// Returns a sequence of bits in a value using the number of bits to get and the index of where to start retrieving the bits.
        /// </summary>
        /// <param name="value">The value whose bits to get.</param>
        /// <param name="numBitsToGet">The number of bits to get in the value.</param>
        /// <param name="bitStartIndex">The index of where to start retrieving the bits in the value.</param>
        /// <returns></returns>
        public static uint GetBits(uint value, int numBitsToGet, int bitStartIndex)
        {
            uint bitsToGet = (uint)FillBits(numBitsToGet);
            return (value & (bitsToGet << bitStartIndex)) >> bitStartIndex;
        }

        /// <summary>
        /// Returns a sequence of bits in a value using the number of bits to get and the index of where to start retrieving the bits.
        /// </summary>
        /// <param name="value">The value whose bits to get.</param>
        /// <param name="numBitsToGet">The number of bits to get in the value.</param>
        /// <param name="bitStartIndex">The index of where to start retrieving the bits in the value.</param>
        /// <returns></returns>
        public static ushort GetBits(ushort value, int numBitsToGet, int bitStartIndex)
        {
            ushort bitsToGet = (ushort)FillBits(numBitsToGet);
            return (ushort)((value & (bitsToGet << bitStartIndex)) >> bitStartIndex);
        }

        /// <summary>
        /// Returns a sequence of bits in a value using the number of bits to get and the index of where to start retrieving the bits.
        /// </summary>
        /// <param name="value">The value whose bits to get.</param>
        /// <param name="numBitsToGet">The number of bits to get in the value.</param>
        /// <param name="bitStartIndex">The index of where to start retrieving the bits in the value.</param>
        /// <returns></returns>
        public static byte GetBits(byte value, int numBitsToGet, int bitStartIndex)
        {
            byte bitsToGet = (byte)FillBits(numBitsToGet);
            return (byte)((value & (bitsToGet << bitStartIndex)) >> bitStartIndex);
        }

        private static ulong FillBits(int numBits)
        {
            ulong value = 0;
            for (int i = 0; i < numBits; i++)
            {
                value |= (ulong)1 << i;
            }
            return value;
        }
    }
}
