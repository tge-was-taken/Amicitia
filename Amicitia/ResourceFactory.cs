namespace Amicitia
{
    using AtlusLibSharp.Common;
    using AtlusLibSharp.Common.FileSystem.Archives;
    using AtlusLibSharp.Persona3.FileSystem.Archives;
    using AtlusLibSharp.Persona3.RenderWare;
    using AtlusLibSharp.SMT3.Graphics;
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
                case SupportedFileType.PAKToolFile:
                    return new PAKToolFileWrapper(text, new PAKToolFile(stream));

                case SupportedFileType.TMXFile:
                    return new TMXFileWrapper(text, TMXFile.LoadFrom(stream, false));

                case SupportedFileType.SPRFile:
                    return new SPRFileWrapper(text, SPRFile.LoadFrom(stream, false));

                case SupportedFileType.SPR4File:
                    return new SPR4FileWrapper(text, SPR4File.LoadFrom(stream, false));

                case SupportedFileType.BVPArchiveFile:
                    return new BVPArchiveFileWrapper(text, new BVPArchiveFile(stream));

                case SupportedFileType.ListArchiveFile:
                    return new ListArchiveFileWrapper(text, new ListArchiveFile(stream));

                case SupportedFileType.RMDScene:
                    return new RMDSceneWrapper(text, new RMDScene(stream, false));

                default:
                    return new ResourceWrapper(text, new GenericBinaryFile(stream));
            }
        }
    }
}
