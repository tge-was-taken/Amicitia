using AtlusLibSharp.Field;

namespace Amicitia
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Drawing;

    using AtlusLibSharp.Utilities;
    using AtlusLibSharp.FileSystems.BVP;
    using AtlusLibSharp.Graphics.RenderWare;
    using AtlusLibSharp.FileSystems.ListArchive;
    using AtlusLibSharp.FileSystems.PAKToolArchive;
    using AtlusLibSharp.FileSystems.AMD;
    using AtlusLibSharp.Graphics.TMX;
    using AtlusLibSharp.Graphics.SPR;
    using AtlusLibSharp.Graphics.TGA;
    using AtlusLibSharp.IO;
    using Assimp;
    using AtlusLibSharp.FileSystems.ACX;

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
        {
            // Generic formats
            new SupportedFileInfo("Raw data",         SupportedFileType.Resource,        typeof(BinaryFile),         ".*"),
            new SupportedFileInfo("Bitmap",           SupportedFileType.Bitmap,          typeof(Bitmap),             ".png", ".bmp", ".gif", ".ico", ".jpg", ".jpeg", ".jif", ".jfif", ".jfi", ".tiff", ".tif"),
            new SupportedFileInfo("Truevision TARGA", SupportedFileType.TgaFile,         typeof(TgaFile),            ".tga"),
            new SupportedFileInfo("Assimp Model",     SupportedFileType.AssimpModelFile, typeof(Scene),              Assimp.Unmanaged.AssimpLibrary.Instance.GetExtensionList()),

            // Archive formats
            new SupportedFileInfo("Atlus Generic Archive",            SupportedFileType.PakToolArchiveFile, typeof(PakToolArchiveFile), ".bin", ".f00", ".f01", ".p00", ".p01", ".fpc", ".pak", ".pac", ".pack", ".se"),
            new SupportedFileInfo("Atlus Generic List Archive",       SupportedFileType.ListArchiveFile,    typeof(ListArchiveFile),    ".arc", ".bin", ".pak", ".pac", ".abin", ".se", ".pse"),
            new SupportedFileInfo("Persona 3/4 Battle Voice Package", SupportedFileType.BvpArchiveFile,     typeof(BvpFile),            ".bvp"),
            new SupportedFileInfo("Atlus Vita Resource Container",    SupportedFileType.AmdFile,            typeof(AmdFile),            ".amd"),
            new SupportedFileInfo("CRIWare Sound Container",          SupportedFileType.AcxFile,            typeof(AcxFile),            ".acx"),

            // Texture (container) formats
            new SupportedFileInfo("Atlus PS2 Texture",                SupportedFileType.TmxFile,                  typeof(TmxFile),                 ".tmx"),
            new SupportedFileInfo("RenderWare PS2 Texture Container", SupportedFileType.RwTextureDictionaryNode,  typeof(RwTextureDictionaryNode), ".txd"),
            new SupportedFileInfo("RenderWare PS2 Texture",           SupportedFileType.RwTextureNativeNode,      typeof(RwTextureNativeNode),     ".txn"),

            // Sprite
            new SupportedFileInfo("Atlus TMX Sprite Container", SupportedFileType.SprFile,     typeof(SprFile),     ".spr"),
            new SupportedFileInfo("Atlus TGA Sprite Container", SupportedFileType.Spr4File,    typeof(Spr4File),    ".spr4"),
            new SupportedFileInfo("Atlus Sprite Key Frame",     SupportedFileType.SprKeyFrame, typeof(SprKeyFrame), ".sprkf"),

            // Model related formats
            new SupportedFileInfo("Atlus RenderWare Model Data",     SupportedFileType.RmdScene,        typeof(RmdScene),            ".rmd", ".rws"),
            new SupportedFileInfo("RenderWare Clump",                SupportedFileType.RwClumpNode,     typeof(RwClumpNode),         ".dff"),
            new SupportedFileInfo("Atlus RenderWare Node Link",      SupportedFileType.RmdNodeLink,     typeof(RmdNodeLink),         ".nl"),
            new SupportedFileInfo("Atlus RenderWare Node Link List", SupportedFileType.RmdNodeLinkList, typeof(RmdNodeLinkListNode), ".nll"),
            new SupportedFileInfo("RenderWare Node",                 SupportedFileType.RwNode,          typeof(RwNode),              ".rwn"),
            new SupportedFileInfo("Atlus RenderWare Animation",      SupportedFileType.RmdAnimation,    typeof(RmdAnimation),        ".rmdanim"),
            new SupportedFileInfo("RenderWare Geometry",             SupportedFileType.RwGeometryNode,  typeof(RwGeometryNode),      ".geo"),
            new SupportedFileInfo("RenderWare Atomic",               SupportedFileType.RwAtomicNode,    typeof(RwAtomicNode),        ".atm"),

            // Field related formats
            new SupportedFileInfo("Field Camera Parameters",         SupportedFileType.CmrFile,         typeof(CmrFile),             ".cmr"),
            new SupportedFileInfo("Field Object Placement",          SupportedFileType.FbnFile,         typeof(FbnFile),             ".fbn"),
            new SupportedFileInfo("Field Hit Placement",             SupportedFileType.HbnFile,         typeof(HbnFile),             ".hbn"),
        };

        public static string FileFilter { get; }

        public static SupportedFileType GetSupportedFileType(int index)
        {
            if (index == -1)
            {
                return SupportedFileType.Resource;
            }
            else
            {
                return SupportedFileInfos[index].EnumType;
            }
        }

        public static SupportedFileType GetSupportedFileType(Type type)
        {
            if (sTypeToSupportedFileTypeEnum.ContainsKey(type))
                return sTypeToSupportedFileTypeEnum[type];
            else return SupportedFileType.Resource;
        }

        public static SupportedFileInfo GetSupportedFileInfo(int index)
        {
            return SupportedFileInfos[index];
        }

        public static SupportedFileInfo GetSupportedFileInfo(Type type)
        {
            return SupportedFileInfos.Single(x => x.ClassType == type);
        }

        public static SupportedFileInfo GetSupportedFileInfo(SupportedFileType type)
        {
            return SupportedFileInfos.Single(x => x.EnumType == type);
        }

        public static SupportedFileInfo GetSupportedFileInfo(string extension)
        {
            return SupportedFileInfos.Single(x => x.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase));
        }

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
            // TODO: AddEventHandler support for multiple possible support formats, and distinguishing between those ala content based file id checks.
            string ext = Path.GetExtension(name).ToLowerInvariant();
            SupportedFileInfo[] matched = Array.FindAll(SupportedFileInfos, s => s.Extensions.Contains(ext));

            // No matches were found
            if (matched.Length == 0)
                return -1;

            // TODO: Reflection is slow, perhaps speed it up somehow?
            if (matched.Length > 1)
            {
                for (int i = 0; i < matched.Length; i++)
                {
                    Type type = sSupportedFileTypeEnumToType[matched[i].EnumType];
                    MethodInfo methodInfo = type.GetRuntimeMethod("VerifyFileType", new Type[] { typeof(Stream) });
                    bool verifiedSuccess = (bool)methodInfo.Invoke(null, new object[] { stream });
                    if (verifiedSuccess)
                    {
                        return Array.IndexOf(SupportedFileInfos, matched[i]);
                    }
                }

                return -1;
            }
            else
            {
                return Array.IndexOf(SupportedFileInfos, matched[0]);
            }
        }

        public static int GetSupportedFileIndex(object resource)
        {
            var type = resource.GetType();
            return Array.FindIndex(SupportedFileInfos, x => x.ClassType == type);
        }

        public static string GetFilteredFileFilter(params SupportedFileType[] includedTypes)
        {
            string filter = string.Empty;
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>(includedTypes.Length);

            foreach (SupportedFileInfo item in SupportedFileInfos)
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

        private static List<SupportedFileInfo> GetSortedFilteredInfo(List<SupportedFileInfo> unsortedInfo, SupportedFileType[] includedTypes)
        {
            List<SupportedFileInfo> filteredInfo = new List<SupportedFileInfo>(unsortedInfo.Count);
            foreach (SupportedFileType fileType in includedTypes)
            {
                filteredInfo.Add(unsortedInfo.Find(info => info.EnumType == fileType));
            }
            return filteredInfo;
        }

        private static Dictionary<SupportedFileType, Type> GenerateSupportedFileTypeEnumToTypeDictionary()
        {
            var dictionary = new Dictionary<SupportedFileType, Type>();
            foreach (var info in SupportedFileInfos)
            {
                dictionary[info.EnumType] = info.ClassType;
            }

            return dictionary;
        }

        private static string GenerateFileFilter()
        {
            string filter = "All files|*.*|";
            for (int i = 0; i < SupportedFileInfos.Length; i++)
            {
                filter += SupportedFileInfoToFilterString(SupportedFileInfos[i]);

                if (i != SupportedFileInfos.Length - 1)
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
