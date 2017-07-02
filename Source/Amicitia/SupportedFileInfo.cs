using System;

namespace Amicitia
{
    internal class SupportedFileInfo
    {
        public string Description;
        public SupportedFileType EnumType;
        public Type ClassType;
        public string[] Extensions;

        public SupportedFileInfo(string description, SupportedFileType type, Type classType, params string[] extensions)
        {
            Description = description;
            EnumType = type;
            ClassType = classType;
            Extensions = extensions;
        }
    }

    public enum SupportedFileType
    {
        // Default
        Resource = 0,

        // Archive formats
        BvpArchiveFile,
        ListArchiveFile,
        PakToolArchiveFile,
        AmdFile,

        // Texture formats
        RwTextureDictionaryNode,
        RwTextureNativeNode,
        Bitmap,
        SprFile,
        Spr4File,
        TmxFile,
        TgaFile,

        // Model formats
        RmdScene,
        RwClumpNode,
        AssimpModelFile,
        RmdNodeLink,
        RmdNodeLinkList,
        SprKeyFrame,
        RwNode,
        RmdAnimation,
        RwGeometryNode,
        CmrFile,
        FbnFile,
        HbnFile,
        RwAtomicNode,
        AcxFile
    }
}
