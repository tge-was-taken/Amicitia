namespace AtlusLibSharp.Utilities
{
    using System.Runtime.CompilerServices;

    internal static class AlignmentHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Align(long value, int alignment)
        {
            return (value + (alignment - 1)) & ~(alignment - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Align(int value, int alignment)
        {
            return (value + (alignment - 1)) & ~(alignment - 1);
        }
    }
}
