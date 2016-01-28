namespace Amicitia
{
    using AtlusLibSharp.Generic.Archives;
    using AtlusLibSharp.Persona3.Archives;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal static class SupportedFileHandler
    {
        private static readonly SupportedFileInfo[] _supportedFiles = new SupportedFileInfo[]
        {
            new SupportedFileInfo("Atlus Generic Archive",          SupportedFileType.GenericPAK,   false, ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac"),
            new SupportedFileInfo("Atlus PS2 Texture",              SupportedFileType.TMX,          false, ".tmx"),
            new SupportedFileInfo("Portable Network Graphics",      SupportedFileType.PNG,          true , ".png"),
            new SupportedFileInfo("Atlus PS2 Sprite Container",     SupportedFileType.SPR,          false, ".spr"),
            new SupportedFileInfo("Persona 3 Battle Voice Package", SupportedFileType.BVPArchive,   false, ".bvp"),
            new SupportedFileInfo("Atlus Vita Archive",             SupportedFileType.ARCArchive,   false, ".arc", ".bin", ".pak", ".pac")
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
            // TODO: Add support for multiple possible support formats, and distinguishing between those ala content based file type checks.
            SupportedFileInfo[] matched = Array.FindAll(_supportedFiles, s => s.Extensions.Contains(Path.GetExtension(path).ToLowerInvariant()));

            if (matched.Length == 0)
                return -1;

            // TODO: Get rid of this crap
            if (matched.Length > 1)
            {
                if (matched[0].Type == SupportedFileType.GenericPAK && matched[1].Type == SupportedFileType.ARCArchive)
                {
                    if (GenericPAK.VerifyFileType(path))
                        return Array.IndexOf(_supportedFiles, matched[0]);
                    else if (GenericVitaArchive.VerifyFileType(path))
                        return Array.IndexOf(_supportedFiles, matched[1]);
                    else
                        return -1;
                }
                else
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
