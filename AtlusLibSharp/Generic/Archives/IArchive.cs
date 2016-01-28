using System.Collections.Generic;

namespace AtlusLibSharp.Persona3.Archives
{
    public interface IArchive
    {
        int EntryCount { get; }
        IArchiveEntry GetEntry(int index);
    }

    public interface IArchiveEntry
    {
        string Name { get; }
        int DataLength { get; }
        byte[] Data { get; }
    }
}