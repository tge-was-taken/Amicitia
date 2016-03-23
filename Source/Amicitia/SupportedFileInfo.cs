using System;

namespace Amicitia
{
    internal struct SupportedFileInfo
    {
        public string Description;
        public SupportedFileType EnumType;
        public Type ClassType;
        public bool ExportOnly;
        public string[] Extensions;

        public SupportedFileInfo(string description, SupportedFileType type, Type classType, bool exportOnly, params string[] extensions)
        {
            Description = description;
            EnumType = type;
            ClassType = classType;
            ExportOnly = exportOnly;
            Extensions = extensions;
        }
    }

    internal enum SupportedFileType
    {
        // Default
        Resource = 0,

        // Archive formats
        BVPArchiveFile,
        ListArchiveFile,
        PAKToolArchiveFile,
        AMDFile,

        // Texture formats
        RWTextureDictionary,
        RWTextureNative,
        PNGFile,
        SPRFile,
        SPR4File,
        TMXFile,
        TGAFile,

        // Model formats
        RMDScene,
        RWScene,
        DAEFile
    }
}
