namespace AtlusLibSharp.Utilities
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides convenience methods for <see cref="Enum"/> values.
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Less expensive version of <see cref="Enum.HasFlag(Enum)"/> without error checking.
        /// </summary>
        /// <param name="thisEnum">The current <see cref="Enum"/> instance.</param>
        /// <param name="value">The <see cref="Enum"/> used in the query.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagUnchecked(this Enum thisEnum, Enum value)
        {
            long thisEnumValue = Convert.ToInt64(thisEnum);
            long valueEnumValue = Convert.ToInt64(value);
            return ((long)thisEnumValue & (long)valueEnumValue) == valueEnumValue;
        }
    }
}
