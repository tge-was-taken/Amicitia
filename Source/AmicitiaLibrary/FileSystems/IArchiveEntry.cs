namespace AmicitiaLibrary.FileSystems
{
    public interface IArchiveEntry
    {
        byte[] Data { get; set; }

        int DataLength { get; }

        string Name { get; set; }

        IArchiveEntry Create(byte[] data, string name);
    }
}