namespace GameIO
{
    public enum Endian
    {
        Little,
        Big
    }

    public enum StringType
    {
        NullTerminated,
        FixedLength,
        PrefixedLengthByte,
        PrefixedLengthShort,
        PrefixedLengthInt
    }
}
