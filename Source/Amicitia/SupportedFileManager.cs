using AmicitiaLibrary.Field;
using AmicitiaLibrary.Graphics.SPD;
using AmicitiaLibrary.Graphics.SPR6;
using AtlusFileSystemLibrary;
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
        {   //                     Description                        File Type Enum                              Type                             Validator              Instanciator                                        Get Stream                                            Extensions
            // Generic formats
            new SupportedFileInfo("Raw data",                         SupportedFileType.Resource,                 typeof(BinaryFile),              null,                  (s, o, f) => new BinaryFile(s, o),                  o => ((BinaryBase)o).GetMemoryStream(),               ".*"),
            new SupportedFileInfo("DDS image",                        SupportedFileType.DDS,                      typeof(BinaryFile),              null,                  (s, o, f) => new BinaryFile(s, o),                  o => ((BinaryBase)o).GetMemoryStream(),               ".dds"),
            new SupportedFileInfo("Bitmap",                           SupportedFileType.Bitmap,                   typeof(Bitmap),                  null,                  (s, o, f) => new Bitmap(s),                         o => ((BinaryBase)o).GetMemoryStream(),               ".png", ".bmp", ".gif", ".ico", ".jpg", ".jpeg", ".jif", ".jfif", ".jfi", ".tiff", ".tif"),
            new SupportedFileInfo("Truevision TARGA",                 SupportedFileType.TgaFile,                  typeof(TgaFile),                 null,                  (s, o, f) => new TgaFile(s, o),                     o => ((BinaryBase)o).GetMemoryStream(),               ".tga"),
            new SupportedFileInfo("Assimp Model",                     SupportedFileType.AssimpModelFile,          typeof(Scene),                   null,                  null,                                               o => ((BinaryBase)o).GetMemoryStream(),               ".fbx", ".dae", ".obj"),

            // Archive formats
            new SupportedFileInfo("Atlus Generic Archive",            SupportedFileType.PakArchiveFile,           typeof(PAKFileSystem),           (s, f) => PAKFileSystem.IsValid(s), OpenPAKFileSystemFile,                 o => (MemoryStream)((PAKFileSystem)o).Save(),         ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac", ".pack", ".se", ".arc", ".abin", ".se", ".pse"),
            new SupportedFileInfo("Persona 3/4 Battle Voice Package", SupportedFileType.BvpArchiveFile,           typeof(BvpFile),                 null,                  (s, o, f) => new BvpFile(s, o),                     o => ((BinaryBase)o).GetMemoryStream(),               ".bvp"),
            new SupportedFileInfo("Atlus Vita Resource Container",    SupportedFileType.AmdFile,                  typeof(AmdFile),                 null,                  (s, o, f) => new AmdFile(s, o),                     o => ((BinaryBase)o).GetMemoryStream(),               ".amd"),
            new SupportedFileInfo("CRIWare Sound Container",          SupportedFileType.AcxFile,                  typeof(ACXFileSystem),           null,                  OpenACXFileSystemFile,                              o => (MemoryStream)((ACXFileSystem)o).Save(),         ".acx"),

            // Texture (container) formats
            new SupportedFileInfo("Atlus PS2 Texture",                SupportedFileType.TmxFile,                  typeof(TmxFile),                 null,                 (s, o, f) => new TmxFile(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".tmx"),
            new SupportedFileInfo("RenderWare PS2 Texture Container", SupportedFileType.RwTextureDictionaryNode,  typeof(RwTextureDictionaryNode), null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".txd"),
            new SupportedFileInfo("RenderWare PS2 Texture",           SupportedFileType.RwTextureNativeNode,      typeof(RwTextureNativeNode),     null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".txn"),

            // Sprite
            new SupportedFileInfo("Atlus P3/P4 Sprite Container",     SupportedFileType.SprFile,                  typeof(SprFile),                 null,                 (s, o, f) => new SprFile(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".spr"),
            new SupportedFileInfo("Atlus P4D Sprite Container",       SupportedFileType.Spr4File,                 typeof(Spr4File),                null,                 (s, o, f) => new Spr4File(s, o),                     o => ((BinaryBase)o).GetMemoryStream(),               ".spr4"),
            new SupportedFileInfo("Atlus P3/P5D Sprite Container",    SupportedFileType.Spr6File,                 typeof(Spr6File),                null,                 (s, o, f) => new Spr6File(s, o),                     o => ((Spr6File)o).Save(),                            ".spr6"),
            new SupportedFileInfo("Atlus P3/P5D Sprite Texture",      SupportedFileType.Spr6Sprite,               typeof(Spr6Sprite),              null,                 (s, o, f) => new Spr6Sprite(s, o),                   o => ((Spr6Sprite)o).Save(),                          ".spr6spr"),
            new SupportedFileInfo("Atlus P3/P5D Sprite Panel",        SupportedFileType.Spr6Panel,                typeof(Spr6Panel),               null,                 (s, o, f) => new Spr6Panel(s, o),                    o => ((Spr6Panel)o).Save(),                           ".spr6pnl"),
            new SupportedFileInfo("Atlus P3/P5D Sprite Texture",      SupportedFileType.Spr6Texture,              typeof(Spr6Texture),             null,                 (s, o, f) => new Spr6Texture(s, o),                  o => ((Spr6Texture)o).Save(),                         ".spr6tex"),
            new SupportedFileInfo("Atlus Sprite",                     SupportedFileType.SprSprite,                typeof(SprSprite),               null,                 (s, o, f) => new SprSprite(s, o),                    o => ((BinaryBase)o).GetMemoryStream(),               ".sprt"),
            new SupportedFileInfo("Atlus P5 Sprite Container",        SupportedFileType.SpdFile,                  typeof(SpdFile),                 null,                 (s, o, f) => new SpdFile(s, o),                      o => ((SpdFile)o).Save(),                             ".spd"),
            new SupportedFileInfo("Atlus P5 Sprite",                  SupportedFileType.SpdSprite,                typeof(SpdSprite),               null,                 (s, o, f) => new SpdSprite(s, o),                    o => ((SpdSprite)o).Save(),                           ".spdspr"),
            new SupportedFileInfo("Atlus P5 Sprite Texture",          SupportedFileType.SpdTexture,               typeof(SpdTexture),              null,                 (s, o, f) => new SpdTexture(s, o),                   o => ((SpdTexture)o).Save(),                          ".spdtex"),

            // Model related formats
            new SupportedFileInfo("Atlus RenderWare Model Data",      SupportedFileType.RmdScene,                 typeof(RmdScene),                null,                 (s, o, f) => new RmdScene(s, o),                     o => ((BinaryBase)o).GetMemoryStream(),               ".rmd", ".rws"),
            new SupportedFileInfo("RenderWare Clump",                 SupportedFileType.RwClumpNode,              typeof(RwClumpNode),             null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".dff"),
            new SupportedFileInfo("Atlus RenderWare Node Link",       SupportedFileType.RmdNodeLink,              typeof(RmdNodeLink),             null,                 (s, o, f) => new RmdNodeLink(s),                     o => ((BinaryBase)o).GetMemoryStream(),               ".nl"),
            new SupportedFileInfo("Atlus RenderWare Node Link List",  SupportedFileType.RmdNodeLinkList,          typeof(RmdNodeLinkListNode),     null,                 (s, o, f) => new RmdNodeLinkListNode(s),             o => ((BinaryBase)o).GetMemoryStream(),               ".nll"),
            new SupportedFileInfo("RenderWare Node",                  SupportedFileType.RwNode,                   typeof(RwNode),                  null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".rwn"),
            new SupportedFileInfo("Atlus RenderWare Animation",       SupportedFileType.RmdAnimation,             typeof(RmdAnimation),            null,                 (s, o, f) => new RmdAnimation(s, o),                 o => ((BinaryBase)o).GetMemoryStream(),               ".rmdanm"),
            new SupportedFileInfo("RenderWare Geometry",              SupportedFileType.RwGeometryNode,           typeof(RwGeometryNode),          null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".geo"),
            new SupportedFileInfo("RenderWare Atomic",                SupportedFileType.RwAtomicNode,             typeof(RwAtomicNode),            null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".atm"),
            new SupportedFileInfo("RenderWare Animation",             SupportedFileType.RwAnimationNode,          typeof(RwAnimationNode),         null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".anm"),
            new SupportedFileInfo("RenderWare Material",              SupportedFileType.RwMaterial,               typeof(RwMaterial),              null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".mat"),
            new SupportedFileInfo("RenderWare Texture Reference",     SupportedFileType.RwTextureReferenceNode,   typeof(RwTextureReferenceNode),  null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".trf"),
            new SupportedFileInfo("RenderWare User Data List",        SupportedFileType.RwUserDataList,           typeof(RwUserDataList),          null,                 (s, o, f) => RwNode.Load(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".udl"),

            // Field related formats
            new SupportedFileInfo("Field Camera Parameters",          SupportedFileType.CmrFile,                  typeof(CmrFile),                 null,                 (s, o, f) => new CmrFile(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".cmr"),
            new SupportedFileInfo("Field Object Placement",           SupportedFileType.FbnFile,                  typeof(FbnFile),                 null,                 (s, o, f) => new FbnFile(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".fbn"),
            new SupportedFileInfo("Field Hit Placement",              SupportedFileType.HbnFile,                  typeof(HbnFile),                 null,                 (s, o, f) => new HbnFile(s, o),                      o => ((BinaryBase)o).GetMemoryStream(),               ".hbn"),
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

        /// <summary>
        /// Get supported file info by type. Supports 1 level of inheritance.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SupportedFileInfo GetSupportedFileInfo( Type type )
        {
            var info = SupportedFileInfos.FirstOrDefault( x => x.ClassType == type );
            while ( info == null )
            {
                info = SupportedFileInfos.FirstOrDefault( x => x.ClassType.BaseType == type );
            }

            return info;
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
            var extension = Path.GetExtension( name )?.Trim().ToLowerInvariant();
            var matches = Array.FindAll( SupportedFileInfos, s => s.Extensions.Contains( extension ) );

            // No matches were found
            if ( matches.Length == 0 )
                return -1;

            if ( matches.Length > 1 )
            {
                foreach ( var match in matches )
                {
                    if ( match.Validator( stream, name ) )
                        Array.IndexOf( SupportedFileInfos, match );
                }
            }
            else if ( matches[0].Validator(stream, name ) )
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

        private static PAKFileSystem OpenPAKFileSystemFile( Stream stream, bool leaveOpen, string fileName )
        {
            var pak = new PAKFileSystem();
            LoadFileSystem<PAKFileSystem, string>( pak, stream, leaveOpen, fileName );
            return pak;
        }

        private static ACXFileSystem OpenACXFileSystemFile( Stream stream, bool leaveOpen, string fileName )
        {
            var acx = new ACXFileSystem();
            LoadFileSystem<ACXFileSystem, int>( acx, stream, leaveOpen, fileName );
            return acx;
        }

        private static void LoadFileSystem<T, T2>( T fs, Stream stream, bool leaveOpen, string fileName ) where T : IFileSystem<T2>
        {
            if ( stream is FileStream )
            {
                // Hack: close stream & load as file
                fs.Dispose();
                fs.Load( fileName );
            }
            else
            {
                fs.Load( stream, ownsStream: !leaveOpen );
            }
        }
    }
}
