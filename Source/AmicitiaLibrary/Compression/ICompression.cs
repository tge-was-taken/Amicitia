namespace AtlusLibSharp.Compression
{
    // TODO
    public interface ICompression
    {
        byte[] Compress(byte[] source);
        byte[] Decompress(byte[] source);
    }
}
