using System;
using System.Diagnostics;
using System.IO;

namespace Amicitia
{
    internal class SupportedFileInfo
    {
        public string Description;
        public SupportedFileType EnumType;
        public Type ClassType;
        public Func<Stream, string, bool> Validator;
        public Func<Stream, bool, string, object> Instantiator;
        public Func<object, MemoryStream> GetStream;
        public string[] Extensions;

        public SupportedFileInfo(string description, SupportedFileType type, Type classType, Func<Stream, string, bool> validator, Func<Stream, bool, string, object> instantiator, Func<object, MemoryStream> getStream, params string[] extensions)
        {
            Description = description;
            EnumType = type;
            ClassType = classType;
            Validator = validator ?? Validate;
            Instantiator = instantiator;
            GetStream = o => GetStreamWrapper( getStream, o );
            Extensions = extensions;
        }

        private bool Validate( Stream stream, string fileName )
        {
            var info = SupportedFileManager.GetSupportedFileInfo( ClassType );
            if ( info == null )
                return false;

            bool valid = true;

            try
            {
                try
                {
                    info.Instantiator( stream, true, fileName );
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

        private MemoryStream GetStreamWrapper( Func<object, MemoryStream> getStream, object obj )
        {
            var stream = getStream( obj );
            stream.Position = 0;
            Trace.Assert( stream.CanRead, "Stream returned by GetStream is closed" );
            return stream;
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
        SprSprite,
        RwNode,
        RmdAnimation,
        RwGeometryNode,
        CmrFile,
        FbnFile,
        HbnFile,
        RwAtomicNode,
        AcxFile,
        RwAnimationNode,
        RwMaterial,
        RwTextureReferenceNode,
        RwUserDataList
    }
}
