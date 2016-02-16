namespace AtlusLibSharp.Common.Utilities
{
    using System;

    public static class EnumExtension
    {
        public static bool HasFlagFast(this Enum thisEnum, Enum value)
        {
            int thisEnumValue = Convert.ToInt32(thisEnum);
            int valueEnumValue = Convert.ToInt32(value);
            return (thisEnumValue & valueEnumValue) == valueEnumValue;
        }
    }
}
