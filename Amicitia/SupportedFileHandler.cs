namespace Amicitia
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using AtlusLibSharp.Common.FileSystem.Archives;
    using AtlusLibSharp.Persona3.FileSystem.Archives;
    using AtlusLibSharp.SMT3.Graphics;
    using AtlusLibSharp.Persona3.RenderWare;

    internal static class SupportedFileHandler
    {
        private static readonly SupportedFileInfo[] _supportedFiles = new SupportedFileInfo[]
        {
            // Non-openable formats
            new SupportedFileInfo("Portable Network Graphics",          SupportedFileType.PNGFile,              true,  ".png"),

            // Archive formats
            new SupportedFileInfo("Atlus Generic Archive",              SupportedFileType.PAKToolFile,          false, ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac", ".pack"),
            new SupportedFileInfo("Atlus Generic List Archive",         SupportedFileType.ListArchiveFile,      false, ".arc", ".bin", ".pak", ".pac"),
            new SupportedFileInfo("Persona 3/4 Battle Voice Package",   SupportedFileType.BVPArchiveFile,       false, ".bvp"),

            // Texture (container) formats
            new SupportedFileInfo("Atlus PS2 Texture",                  SupportedFileType.TMXFile,              false, ".tmx"),
            new SupportedFileInfo("Atlus PS2 Sprite Container",         SupportedFileType.SPRFile,              false, ".spr"),
            new SupportedFileInfo("Atlus TGA Sprite Container",         SupportedFileType.SPR4File,             false, ".spr4"),
            new SupportedFileInfo("RenderWare PS2 Texture Container",   SupportedFileType.RWTextureDictionary,  false, ".txd"),
            new SupportedFileInfo("RenderWare PS2 Texture",             SupportedFileType.RWTextureNative,      false, ".txn"),

            // Model formats
            new SupportedFileInfo("RenderWare Model",                   SupportedFileType.RMDScene,             false, ".rmd", ".rws", ".dff")
        };

        private static readonly Dictionary<SupportedFileType, Type> _supportedFileTypeEnumToType = new Dictionary<SupportedFileType, Type>()
        {
            { SupportedFileType.PAKToolFile,            typeof(PAKToolFile) },
            { SupportedFileType.BVPArchiveFile,         typeof(BVPArchiveFile) },
            { SupportedFileType.ListArchiveFile,        typeof(ListArchiveFile) },
            { SupportedFileType.TMXFile,                typeof(TMXFile) },
            { SupportedFileType.SPRFile,                typeof(SPRFile) },
            { SupportedFileType.SPR4File,               typeof(SPR4File) },
            { SupportedFileType.RMDScene,               typeof(RMDScene) },
            { SupportedFileType.RWTextureDictionary,    typeof(RWTextureDictionary) },
            { SupportedFileType.RWTextureNative,        typeof(RWTextureNative) }
        };

        private static readonly string _fileFilter;

        static SupportedFileHandler()
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
                return SupportedFileType.Default;
            }
            else
            {
                return _supportedFiles[index].Type;
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
            SupportedFileInfo[] matched = Array.FindAll(_supportedFiles, s => s.Extensions.Contains(ext));

            // No matches were found
            if (matched.Length == 0)
                return -1;

            // TODO: Reflection is slow, perhaps speed it up somehow?
            if (matched.Length > 1)
            {
                for (int i = 0; i < matched.Length; i++)
                {
                    Type type = _supportedFileTypeEnumToType[matched[i].Type];
                    MethodInfo methodInfo = type.GetRuntimeMethod("VerifyFileType", new Type[] { typeof(Stream) });
                    bool verifiedSuccess = (bool)methodInfo.Invoke(null, new object[] { stream });
                    if (verifiedSuccess)
                    {
                        return Array.IndexOf(_supportedFiles, matched[i]);
                    }
                }

                return -1;
            }
            else
            {
                return Array.IndexOf(_supportedFiles, matched[0]);
            }
        }

        public static string GetFilteredFileFilter(params SupportedFileType[] includedTypes)
        {
            string filter = string.Empty;
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>(includedTypes.Length);

            foreach (SupportedFileInfo item in _supportedFiles)
            {
                if (includedTypes.Contains(item.Type))
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
                filteredInfo.Add(unsortedInfo.Find(info => info.Type == fileType));
            }
            return filteredInfo;
        }

        private static string GetFileFilter()
        {
            string filter = "All files|*.*|";
            for (int i = 0; i < _supportedFiles.Length; i++)
            {
                if (_supportedFiles[i].ExportOnly == true)
                    continue;

                filter += SupportedFileInfoToFilterString(_supportedFiles[i]);

                if (i != _supportedFiles.Length - 1)
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
