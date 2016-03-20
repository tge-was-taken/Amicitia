namespace Amicitia
{
    using AtlusLibSharp.FileSystems.BVP;
    using AtlusLibSharp.Graphics.RenderWare;
    using ResourceWrappers;
    using System.IO;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;
    using AtlusLibSharp.Graphics.SPR;
    using AtlusLibSharp.Graphics.TMX;
    using AtlusLibSharp.Graphics.TGA;
    using AtlusLibSharp.IO;
    using AtlusLibSharp.FileSystems.AMD;

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
                // Archive formats
                case SupportedFileType.BVPArchiveFile:
                    return new BVPFileWrapper(text, new BVPFile(stream, false));

                case SupportedFileType.ListArchiveFile:
                    return new ListArchiveFileWrapper(text, new ListArchiveFile(stream));

                case SupportedFileType.PAKToolArchiveFile:
                    return new PAKToolFileWrapper(text, new PAKToolArchiveFile(stream));

                case SupportedFileType.AMDFile:
                    return new AMDFileWrapper(text, new AMDFile(stream));

                // Texture formats
                case SupportedFileType.RWTextureDictionary:
                    return new RWTextureDictionaryWrapper(text, (RWTextureDictionary)RWNode.Load(stream));

                case SupportedFileType.RWTextureNative:
                    return new RWTextureNativeWrapper((RWTextureNative)RWNode.Load(stream));

                case SupportedFileType.SPRFile:
                    return new SPRFileWrapper(text, SPRFile.LoadFrom(stream, false));

                case SupportedFileType.SPR4File:
                    return new SPR4FileWrapper(text, SPR4File.LoadFrom(stream, false));

                case SupportedFileType.TMXFile:
                    return new TMXFileWrapper(text, TMXFile.Load(stream, false));

                case SupportedFileType.TGAFile:
                    return new TGAFileWrapper(text, new TGAFile(stream));

                // Model formats
                case SupportedFileType.RMDScene:
                    return new RMDSceneWrapper(text, new RMDScene(stream, false));

                default:
                    return new ResourceWrapper(text, new GenericBinaryFile(stream));
            }
        }
    }
}
