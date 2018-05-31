using System;
using System.IO;

namespace Amicitia
{
    internal class SupportedFileInfo
    {
        public string Description;
        public SupportedFileType EnumType;
        public Type ClassType;
        public Func<Stream, bool, object> Instantiator;
        public string[] Extensions;

        public SupportedFileInfo(string description, SupportedFileType type, Type classType, Func<Stream, bool, object> instantiator, params string[] extensions)
        {
            Description = description;
            EnumType = type;
            ClassType = classType;
            Instantiator = instantiator;
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
        AcxFile,
        RwAnimationNode
    }
}
