namespace AtlusLibSharp.Common.Utilities
{
    public static class AlignmentHelper
    {
        public static long Align(long value, int alignment)
        {
            return (value + (alignment - 1)) & ~(alignment - 1);
        }

        public static int Align(int value, int alignment)
        {
            return (value + (alignment - 1)) & ~(alignment - 1);
        }
    }
}
