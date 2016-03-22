namespace Amicitia
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using AtlusLibSharp.Utilities;
    using AtlusLibSharp.FileSystems.BVP;
    using AtlusLibSharp.Graphics.RenderWare;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;
    using AtlusLibSharp.Graphics.TMX;
    using AtlusLibSharp.Graphics.SPR;
    using AtlusLibSharp.Graphics.TGA;
    using AtlusLibSharp.IO;
    using System.Drawing;
    using Assimp;
    using AtlusLibSharp.FileSystems.AMD;

    internal static class SupportedFileManager
    {
        private static readonly SupportedFileInfo[] _supportedFileInfos = new SupportedFileInfo[]
        {
            // Export only formats
            new SupportedFileInfo("Raw data",                           SupportedFileType.Resource,             typeof(GenericBinaryFile),  false, ".*"),
            new SupportedFileInfo("Portable Network Graphics",          SupportedFileType.PNGFile,              typeof(Bitmap),             true,  ".png"),
            new SupportedFileInfo("Truevision TARGA",                   SupportedFileType.TGAFile,              typeof(TGAFile),            false, ".tga"),
            new SupportedFileInfo("COLLADA DAE",                        SupportedFileType.DAEFile,              typeof(Scene),              false, ".dae"),

            // Archive formats
            new SupportedFileInfo("Atlus Generic Archive",              SupportedFileType.PAKToolArchiveFile,   typeof(PAKToolArchiveFile), false, ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac", ".pack", ".se"),
            new SupportedFileInfo("Atlus Generic List Archive",         SupportedFileType.ListArchiveFile,      typeof(ListArchiveFile),    false, ".arc", ".bin", ".pak", ".pac", ".abin", ".se", ".pse"),
            new SupportedFileInfo("Persona 3/4 Battle Voice Package",   SupportedFileType.BVPArchiveFile,       typeof(BVPFile),            false, ".bvp"),
            new SupportedFileInfo("Atlus Vita Resource Container",      SupportedFileType.AMDFile,              typeof(AMDFile),            false, ".amd"),

            // Texture (container) formats
            new SupportedFileInfo("Atlus PS2 Texture",                  SupportedFileType.TMXFile,              typeof(TMXFile),            false, ".tmx"),
            new SupportedFileInfo("Atlus TMX Sprite Container",         SupportedFileType.SPRFile,              typeof(SPRFile),            false, ".spr"),
            new SupportedFileInfo("Atlus TGA Sprite Container",         SupportedFileType.SPR4File,             typeof(SPR4File),           false, ".spr4"),
            new SupportedFileInfo("RenderWare PS2 Texture Container",   SupportedFileType.RWTextureDictionary,  typeof(RWTextureDictionary),false, ".txd"),
            new SupportedFileInfo("RenderWare PS2 Texture",             SupportedFileType.RWTextureNative,      typeof(RWTextureNative),    false, ".txn"),

            // Model formats
            new SupportedFileInfo("Atlus RenderWare Scene Container",   SupportedFileType.RMDScene,             typeof(RMDScene),           false, ".rmd", ".rws"),
            new SupportedFileInfo("RenderWare Scene",                   SupportedFileType.RWScene,              typeof(RWScene),            false, ".dff")
        };

        private static readonly Dictionary<SupportedFileType, Type> _supportedFileTypeEnumToType = new Dictionary<SupportedFileType, Type>()
        {
            // Archive formats
            { SupportedFileType.BVPArchiveFile,         typeof(BVPFile) },
            { SupportedFileType.ListArchiveFile,        typeof(ListArchiveFile) },
            { SupportedFileType.PAKToolArchiveFile,     typeof(PAKToolArchiveFile) },

            // Texture formats
            { SupportedFileType.RWTextureDictionary,    typeof(RWTextureDictionary) },
            { SupportedFileType.RWTextureNative,        typeof(RWTextureNative) },
            { SupportedFileType.SPRFile,                typeof(SPRFile) },
            { SupportedFileType.SPR4File,               typeof(SPR4File) },
            { SupportedFileType.TMXFile,                typeof(TMXFile) },
            { SupportedFileType.TGAFile,                typeof(TGAFile) },

            // Model formats
            { SupportedFileType.RMDScene,               typeof(RMDScene) },
            { SupportedFileType.RWScene,                typeof(RWScene) },
        };

        private static readonly Dictionary<Type, SupportedFileType> _TypeToSupportedFileTypeEnum = _supportedFileTypeEnumToType.Reverse();

        private static readonly string _fileFilter;

        static SupportedFileManager()
        { 
            _fileFilter = GetFileFilter();
        }

        // Properties
        public static string FileFilter
        {
            get { return _fileFilter; }
        }

        public static SupportedFileType GetType(int index)
        {
            if (index == -1)
            {
                return SupportedFileType.Resource;
            }
            else
            {
                return _supportedFileInfos[index].EnumType;
            }
        }

        // Public Methods
        public static int GetSupportedFileIndex(string path)
        {
            int idx = -1;
            using (FileStream stream = File.OpenRead(path))
            {
                idx = GetSupportedFileIndex(path, stream);
            }

            return idx;
        }

        public static int GetSupportedFileIndex(string name, Stream stream)
        {
            // TODO: Add support for multiple possible support formats, and distinguishing between those ala content based file type checks.
            string ext = Path.GetExtension(name).ToLowerInvariant();
            SupportedFileInfo[] matched = Array.FindAll(_supportedFileInfos, s => s.Extensions.Contains(ext));

            // No matches were found
            if (matched.Length == 0)
                return -1;

            // TODO: Reflection is slow, perhaps speed it up somehow?
            if (matched.Length > 1)
            {
                for (int i = 0; i < matched.Length; i++)
                {
                    Type type = _supportedFileTypeEnumToType[matched[i].EnumType];
                    MethodInfo methodInfo = type.GetRuntimeMethod("VerifyFileType", new Type[] { typeof(Stream) });
                    bool verifiedSuccess = (bool)methodInfo.Invoke(null, new object[] { stream });
                    if (verifiedSuccess)
                    {
                        return Array.IndexOf(_supportedFileInfos, matched[i]);
                    }
                }

                return -1;
            }
            else
            {
                return Array.IndexOf(_supportedFileInfos, matched[0]);
            }
        }

        public static string GetFilteredFileFilter(params SupportedFileType[] includedTypes)
        {
            string filter = string.Empty;
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>(includedTypes.Length);

            foreach (SupportedFileInfo item in _supportedFileInfos)
            {
                if (includedTypes.Contains(item.EnumType))
                {
                    filteredInfo.Add(item);
                }
            }

            filteredInfo = GetSortedFilteredInfo(filteredInfo, includedTypes);

            for (int i = 0; i < filteredInfo.Count; i++)
            {
                filter += SupportedFileInfoToFilterString(filteredInfo[i]);

                if (i != filteredInfo.Count - 1)
                {
                    // For every entry that isn't the last, add a seperator
                    filter += "|";
                }
            }

            return filter;
        }

        // Private Methods
        private static List<SupportedFileInfo> GetSortedFilteredInfo(List<SupportedFileInfo> unsortedInfo, SupportedFileType[] includedTypes)
        {
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>(unsortedInfo.Count);
            foreach (SupportedFileType fileType in includedTypes)
            {
                filteredInfo.Add(unsortedInfo.Find(info => info.EnumType == fileType));
            }
            return filteredInfo;
        }

        private static string GetFileFilter()
        {
            string filter = "All files|*.*|";
            for (int i = 0; i < _supportedFileInfos.Length; i++)
            {
                if (_supportedFileInfos[i].ExportOnly == true)
                    continue;

                filter += SupportedFileInfoToFilterString(_supportedFileInfos[i]);

                if (i != _supportedFileInfos.Length - 1)
                {
                    // For every entry that isn't the last, add a seperator
                    filter += "|";
                }
            }
            return filter;
        }

        private static string SupportedFileInfoToFilterString(SupportedFileInfo info)
        {
            string filter = info.Description + "|";
            for (int i = 0; i < info.Extensions.Length; i++)
            {
                filter += "*" + info.Extensions[i];
                if (i != info.Extensions.Length - 1)
                {
                    // For every entry that isn't the last, add a seperator
                    filter += ";";
                }
            }
            return filter;
        }
    }
}
