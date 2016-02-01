namespace Amicitia
{
    using AtlusLibSharp;
    using AtlusLibSharp.Generic.Archives;
    using AtlusLibSharp.Persona3.Archives;
    using AtlusLibSharp.SMT3.ChunkResources.Graphics;
    using ResourceWrappers;
    using System.IO;

    internal static class ResourceFactory
    {
        public static ResourceWrapper GetResource(string path)
        {
            return GetResource(Path.GetFileName(path), new FileStream(path, FileMode.Open), SupportedFileHandler.GetSupportedFileIndex(path));
        }

        public static ResourceWrapper GetResource(string text, Stream stream)
        {
            return GetResource(text, stream, SupportedFileHandler.GetSupportedFileIndex(text, stream));
        }

        public static ResourceWrapper GetResource(string text, Stream stream, int supportedFileIndex)
        {
            switch (SupportedFileHandler.GetType(supportedFileIndex))
            {
                case SupportedFileType.GenericPAK:
                    return new GenericPAKFileWrapper(text, new GenericPAK(stream));
                case SupportedFileType.TMX:
                    return new TMXWrapper(text, TMXFile.LoadFrom(stream, false));
                case SupportedFileType.SPR0:
                    return new SPRWrapper(text, SPRFile.LoadFrom(stream, false));
                case SupportedFileType.SPR4:
                    return new SPR4Wrapper(text, SPR4File.LoadFrom(stream, false));
                case SupportedFileType.BVPArchive:
                    return new BVPArchiveWrapper(text, new BVPArchive(stream));
                case SupportedFileType.ARCArchive:
                    return new ARCArchiveWrapper(text, new GenericPSVitaArchive(stream));
                default:
                    return new ResourceWrapper(text, new GenericBinaryFile(stream));
            }
        }
    }
}
