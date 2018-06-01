using AmicitiaLibrary.Field;
using AtlusFileSystemLibrary.FileSystems.ACX;

namespace Amicitia
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Drawing;

    using AmicitiaLibrary.Utilities;
    using AmicitiaLibrary.FileSystems.BVP;
    using AmicitiaLibrary.Graphics.RenderWare;
    using AmicitiaLibrary.FileSystems.AMD;
    using AmicitiaLibrary.Graphics.TMX;
    using AmicitiaLibrary.Graphics.SPR;
    using AmicitiaLibrary.Graphics.TGA;
    using AmicitiaLibrary.IO;
    using Assimp;

    using AtlusFileSystemLibrary.FileSystems.PAK;

    internal static class SupportedFileManager
    {
        private static readonly Dictionary<SupportedFileType, Type> sSupportedFileTypeEnumToType;
        private static readonly Dictionary<Type, SupportedFileType> sTypeToSupportedFileTypeEnum;

        static SupportedFileManager()
        {
            sSupportedFileTypeEnumToType = GenerateSupportedFileTypeEnumToTypeDictionary();
            sTypeToSupportedFileTypeEnum = sSupportedFileTypeEnumToType.Reverse();
            FileFilter = GenerateFileFilter();
        }

        // Properties
        public static SupportedFileInfo[] SupportedFileInfos { get; } =
        {   //                     Description                        File Type Enum                              Type                             Validator              Instanciator                                        Extensions
            // Generic formats
            new SupportedFileInfo("Raw data",                         SupportedFileType.Resource,                 typeof(BinaryFile),              null,                  (s, o) => new BinaryFile(s, o),                     ".*"),
            new SupportedFileInfo("Bitmap",                           SupportedFileType.Bitmap,                   typeof(Bitmap),                  null,                  (s, o) => new Bitmap(s),                            ".png", ".bmp", ".gif", ".ico", ".jpg", ".jpeg", ".jif", ".jfif", ".jfi", ".tiff", ".tif"),
            new SupportedFileInfo("Truevision TARGA",                 SupportedFileType.TgaFile,                  typeof(TgaFile),                 null,                  (s, o) => new TgaFile(s, o),                        ".tga"),
            new SupportedFileInfo("Assimp Model",                     SupportedFileType.AssimpModelFile,          typeof(Scene),                   null,                  null,                                               ".fbx", ".dae", ".obj"),

            // Archive formats
            new SupportedFileInfo("Atlus Generic Archive",            SupportedFileType.PakArchiveFile,           typeof(PAKFileSystem),           PAKFileSystem.IsValid, OpenPAKFileSystemFile,                              ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac", ".pack", ".se", ".arc", ".abin", ".se", ".pse"),
            new SupportedFileInfo("Persona 3/4 Battle Voice Package", SupportedFileType.BvpArchiveFile,           typeof(BvpFile),                 null,                  (s, o) => new BvpFile(s, o),                        ".bvp"),
            new SupportedFileInfo("Atlus Vita Resource Container",    SupportedFileType.AmdFile,                  typeof(AmdFile),                 null,                  (s, o) => new AmdFile(s, o),                        ".amd"),
            new SupportedFileInfo("CRIWare Sound Container",          SupportedFileType.AcxFile,                  typeof(ACXFileSystem),           null,                  OpenACXFileSystemFile,                              ".acx"),

            // Texture (container) formats
            new SupportedFileInfo("Atlus PS2 Texture",                SupportedFileType.TmxFile,                  typeof(TmxFile),                 null,                 (s, o) => new TmxFile(s, o),                         ".tmx"),
            new SupportedFileInfo("RenderWare PS2 Texture Container", SupportedFileType.RwTextureDictionaryNode,  typeof(RwTextureDictionaryNode), null,                 RwNode.Load,                                         ".txd"),
            new SupportedFileInfo("RenderWare PS2 Texture",           SupportedFileType.RwTextureNativeNode,      typeof(RwTextureNativeNode),     null,                 RwNode.Load,                                         ".txn"),

            // Sprite
            new SupportedFileInfo("Atlus TMX Sprite Container",       SupportedFileType.SprFile,                  typeof(SprFile),                 null,                 (s, o) => new SprFile(s, o),                         ".spr"),
            new SupportedFileInfo("Atlus TGA Sprite Container",       SupportedFileType.Spr4File,                 typeof(Spr4File),                null,                 (s, o) => new Spr4File(s, o),                        ".spr4"),
            new SupportedFileInfo("Atlus Sprite",                     SupportedFileType.SprSprite,                typeof(SprSprite),               null,                 (s, o) => new SprSprite(s, o),                       ".sprt"),

            // Model related formats
            new SupportedFileInfo("Atlus RenderWare Model Data",      SupportedFileType.RmdScene,                 typeof(RmdScene),                null,                 (s, o) => new RmdScene(s, o),                        ".rmd", ".rws"),
            new SupportedFileInfo("RenderWare Clump",                 SupportedFileType.RwClumpNode,              typeof(RwClumpNode),             null,                 RwNode.Load,                                         ".dff"),
            new SupportedFileInfo("Atlus RenderWare Node Link",       SupportedFileType.RmdNodeLink,              typeof(RmdNodeLink),             null,                 (s, o) => new RmdNodeLink(s),                        ".nl"),
            new SupportedFileInfo("Atlus RenderWare Node Link List",  SupportedFileType.RmdNodeLinkList,          typeof(RmdNodeLinkListNode),     null,                 (s, o) => new RmdNodeLinkListNode(s),                ".nll"),
            new SupportedFileInfo("RenderWare Node",                  SupportedFileType.RwNode,                   typeof(RwNode),                  null,                 RwNode.Load,                                         ".rwn"),
            new SupportedFileInfo("Atlus RenderWare Animation",       SupportedFileType.RmdAnimation,             typeof(RmdAnimation),            null,                 (s, o) => new RmdAnimation(s, o),                    ".rmdanm"),
            new SupportedFileInfo("RenderWare Geometry",              SupportedFileType.RwGeometryNode,           typeof(RwGeometryNode),          null,                 RwNode.Load,                                         ".geo"),
            new SupportedFileInfo("RenderWare Atomic",                SupportedFileType.RwAtomicNode,             typeof(RwAtomicNode),            null,                 RwNode.Load,                                         ".atm"),
            new SupportedFileInfo("RenderWare Animation",             SupportedFileType.RwAnimationNode,          typeof(RwAnimationNode),         null,                 RwNode.Load,                                         ".anm"),
            new SupportedFileInfo("RenderWare Material",              SupportedFileType.RwMaterial,               typeof(RwMaterial),              null,                 RwNode.Load,                                         ".mat"),
            new SupportedFileInfo("RenderWare Texture Reference",     SupportedFileType.RwTextureReferenceNode,   typeof(RwTextureReferenceNode),  null,                 RwNode.Load,                                         ".trf"),
            new SupportedFileInfo("RenderWare User Data List",        SupportedFileType.RwUserDataList,           typeof(RwUserDataList),          null,                 RwNode.Load,                                         ".udl"),

            // Field related formats
            new SupportedFileInfo("Field Camera Parameters",          SupportedFileType.CmrFile,                  typeof(CmrFile),                 null,                 (s, o) => new CmrFile(s, o),                         ".cmr"),
            new SupportedFileInfo("Field Object Placement",           SupportedFileType.FbnFile,                  typeof(FbnFile),                 null,                 (s, o) => new FbnFile(s, o),                         ".fbn"),
            new SupportedFileInfo("Field Hit Placement",              SupportedFileType.HbnFile,                  typeof(HbnFile),                 null,                 (s, o) => new HbnFile(s, o),                         ".hbn"),
        };

        public static string FileFilter { get; }

        public static SupportedFileType GetSupportedFileType( int index )
        {
            if ( index == -1 )
            {
                return SupportedFileType.Resource;
            }
            else
            {
                return SupportedFileInfos[index].EnumType;
            }
        }

        public static SupportedFileType GetSupportedFileType( Type type )
        {
            if ( sTypeToSupportedFileTypeEnum.ContainsKey( type ) )
                return sTypeToSupportedFileTypeEnum[type];
            else
                return SupportedFileType.Resource;
        }

        public static SupportedFileInfo GetSupportedFileInfo( int index )
        {
            return SupportedFileInfos[index];
        }

        public static SupportedFileInfo GetSupportedFileInfo( Type type )
        {
            return SupportedFileInfos.Single( x => x.ClassType == type );
        }

        public static SupportedFileInfo GetSupportedFileInfo( SupportedFileType type )
        {
            return SupportedFileInfos.Single( x => x.EnumType == type );
        }

        public static SupportedFileInfo GetSupportedFileInfo( string extension )
        {
            return SupportedFileInfos.Single( x => x.Extensions.Contains( extension, StringComparer.InvariantCultureIgnoreCase ) );
        }

        public static int GetSupportedFileIndex( string path )
        {
            int idx = -1;
            using ( FileStream stream = File.OpenRead( path ) )
            {
                idx = GetSupportedFileIndex( path, stream );
            }

            return idx;
        }

        public static int GetSupportedFileIndex( string name, Stream stream )
        {
            var extension = Path.GetExtension( name )?.ToLowerInvariant();
            var matches = Array.FindAll( SupportedFileInfos, s => s.Extensions.Contains( extension ) );

            // No matches were found
            if ( matches.Length == 0 )
                return -1;

            if ( matches.Length > 1 )
            {
                foreach ( var match in matches )
                {
                    if ( match.Validator( stream ) )
                        Array.IndexOf( SupportedFileInfos, match );
                }
            }
            else if ( matches[0].Validator(stream) )
            {
                return Array.IndexOf( SupportedFileInfos, matches[0] );
            }

            return -1;
        }

        public static int GetSupportedFileIndex( object resource )
        {
            var type = resource.GetType();
            return Array.FindIndex( SupportedFileInfos, x => x.ClassType == type );
        }

        public static string GetFilteredFileFilter( params SupportedFileType[] includedTypes )
        {
            string filter = string.Empty;
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>( includedTypes.Length );

            foreach ( SupportedFileInfo item in SupportedFileInfos )
            {
                if ( includedTypes.Contains( item.EnumType ) )
                {
                    filteredInfo.Add( item );
                }
            }

            filteredInfo = GetSortedFilteredInfo( filteredInfo, includedTypes );

            for ( int i = 0; i < filteredInfo.Count; i++ )
            {
                filter += SupportedFileInfoToFilterString( filteredInfo[i] );

                if ( i != filteredInfo.Count - 1 )
                {
                    // For every entry that isn't the last, add a seperator
                    filter += "|";
                }
            }

            return filter;
        }

        private static List<SupportedFileInfo> GetSortedFilteredInfo( List<SupportedFileInfo> unsortedInfo, SupportedFileType[] includedTypes )
        {
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>( unsortedInfo.Count );
            foreach ( SupportedFileType fileType in includedTypes )
            {
                filteredInfo.Add( unsortedInfo.Find( info => info.EnumType == fileType ) );
            }
            return filteredInfo;
        }

        private static Dictionary<SupportedFileType, Type> GenerateSupportedFileTypeEnumToTypeDictionary()
        {
            var dictionary = new Dictionary<SupportedFileType, Type>();
            foreach ( var info in SupportedFileInfos )
            {
                dictionary[info.EnumType] = info.ClassType;
            }

            return dictionary;
        }

        private static string GenerateFileFilter()
        {
            string filter = "All files|*.*|";
            for ( int i = 0; i < SupportedFileInfos.Length; i++ )
            {
                filter += SupportedFileInfoToFilterString( SupportedFileInfos[i] );

                if ( i != SupportedFileInfos.Length - 1 )
                {
                    // For every entry that isn't the last, add a seperator
                    filter += "|";
                }
            }
            return filter;
        }

        private static string SupportedFileInfoToFilterString( SupportedFileInfo info )
        {
            string filter = info.Description + "|";
            for ( int i = 0; i < info.Extensions.Length; i++ )
            {
                filter += "*" + info.Extensions[i];
                if ( i != info.Extensions.Length - 1 )
                {
                    // For every entry that isn't the last, add a seperator
                    filter += ";";
                }
            }
            return filter;
        }

        private static PAKFileSystem OpenPAKFileSystemFile( Stream stream, bool leaveOpen )
        {
            var pak = new PAKFileSystem();
            pak.Load( stream, leaveOpen );
            return pak;
        }

        private static ACXFileSystem OpenACXFileSystemFile( Stream stream, bool leaveOpen )
        {
            var acx = new ACXFileSystem();
            acx.Load( stream, leaveOpen );
            return acx;
        }
    }
}
