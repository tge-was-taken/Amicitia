namespace AtlusLibSharp.FileSystems
{
    // TODO
    public interface IArchive
    {
        int EntryCount { get; }
        IArchiveEntry GetEntry(int index);
    }
}