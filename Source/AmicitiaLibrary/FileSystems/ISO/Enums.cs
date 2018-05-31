namespace AmicitiaLibrary.FileSystems.ISO
{
    using System;

    [Flags]
    public enum RecordFlags : byte
    {
        FileRecord = 1 << 0,
        DirectoryRecord = 1 << 1
    }
}
