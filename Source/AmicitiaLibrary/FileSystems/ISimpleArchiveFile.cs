using System.Collections.Generic;

namespace AmicitiaLibrary.FileSystems
{
    public interface ISimpleArchiveFile
    {
        IEnumerable<IArchiveEntry> Entries { get; }

        int EntryCount { get; }

        ISimpleArchiveFile Create(IEnumerable<IArchiveEntry> entries);
    }
}