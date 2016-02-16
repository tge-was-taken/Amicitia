namespace AtlusLibSharp.Common.FileSystem.Archives
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