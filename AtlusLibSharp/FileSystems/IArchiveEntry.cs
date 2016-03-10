namespace AtlusLibSharp.FileSystems
{
    // TODO
    public interface IArchiveEntry
    {
        string Name { get; }
        int DataLength { get; }
        byte[] Data { get; }
    }
}
