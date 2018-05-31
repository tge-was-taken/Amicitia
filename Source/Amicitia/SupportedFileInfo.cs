using System;
using System.IO;

namespace Amicitia
{
    internal class SupportedFileInfo
    {
        public string Description;
        public SupportedFileType EnumType;
        public Type ClassType;
        public Func<Stream, bool> Validator;
        public Func<Stream, bool, object> Instantiator;
        public string[] Extensions;

        public SupportedFileInfo(string description, SupportedFileType type, Type classType, Func<Stream, bool> validator, Func<Stream, bool, object> instantiator, params string[] extensions)
        {
            Description = description;
            EnumType = type;
            ClassType = classType;
            Validator = validator ?? Validate;
            Instantiator = instantiator;
            Extensions = extensions;
        }

        private bool Validate( Stream stream )
        {
            var info = SupportedFileManager.GetSupportedFileInfo( ClassType );
            if ( info == null )
                return false;

            bool valid = true;

            try
            {
                try
                {
                    info.Instantiator( stream, true );
                }
                catch ( Exception )
                {
                    valid = false;
                }
            }
            finally
            {
                stream.Position = 0;
            }

            return valid;
        }
    }

    public enum SupportedFileType
    {
        // Default
        Resource = 0,

        // Archive formats
        BvpArchiveFile,
        PakArchiveFile,
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
