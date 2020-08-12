using System;

namespace Amicitia
{
    using ResourceWrappers;
    using System.IO;
    using AmicitiaLibrary.IO;
    using Amicitia.Utilities;

    internal static class ResourceWrapperFactory
    {
        private static readonly string sResourceWrapperNamespace = typeof(ResourceWrapper<>).Namespace;
        private static readonly string sNamespaceSeperator = ".";
        private static readonly string sResourceWrapperSuffix = "Wrapper";

        public static IResourceWrapper GetResourceWrapper(string path)
        {
            using (var stream = File.OpenRead(path))
                return GetResourceWrapper(Path.GetFileName(path), stream, path);
        }

        public static IResourceWrapper GetResourceWrapper(string text, Stream stream, string filePath = null)
        {
            return GetResourceWrapper(text, stream, SupportedFileManager.GetSupportedFileIndex(text, stream), filePath ?? text);
        }

        public static IResourceWrapper GetResourceWrapper(string text, Stream stream, int supportedFileIndex, string filePath = null)
        {
            if ( stream is FileStream fileStream )
            {
                if ( filePath == null )
                    filePath = fileStream.Name;

                stream = fileStream.ToMemoryStream( leaveOpen: false );
            }

            if (supportedFileIndex == -1)
            {
                return new BinaryFileWrapper(text, new BinaryFile(stream));
            }

            var supportedFileInfo = SupportedFileManager.GetSupportedFileInfo(supportedFileIndex);      
            var resource = supportedFileInfo.Instantiator( stream, false, filePath);

            return GetResourceWrapper(text, resource);
        }

        public static IResourceWrapper GetResourceWrapper(string text, object resource)
        {
            var wrapperType = GetResourceWrapperType(resource);

            object wrapper = null;
            if ( wrapperType != null )
            {
#if !DEBUG
                try
                {
#endif
                    wrapper = Activator.CreateInstance( wrapperType, text, resource );
#if !DEBUG
            }
                catch ( Exception e )
                {
                    Console.WriteLine( e );
                    wrapper = null;
                }   
#endif
            }

            if (wrapperType == null || wrapper == null)
            {
                var binaryFileBase = resource as BinaryBase;
                if ( binaryFileBase != null )
                {
                    wrapper = new BinaryBaseWrapper( text, binaryFileBase );
                }
                else
                {
                    throw new Exception();
                }
            }

            return (IResourceWrapper)wrapper;
        }

        private static Type GetResourceWrapperType(object resource)
        {
            var resourceType = resource.GetType();
            var wrapperType = Type.GetType(GetResourceWrapperTypeName(resourceType), false);
            if (wrapperType == null)
            {
                var type = resourceType.BaseType;
                while (type != null && type != typeof(object))
                {
                    wrapperType = Type.GetType(GetResourceWrapperTypeName(type), false);
                    if (wrapperType != null)
                        break;

                    type = type.BaseType;
                }
            }

            return wrapperType;
        }

        private static string GetResourceWrapperTypeName(Type resourceType)
        {
            return $"{sResourceWrapperNamespace}{sNamespaceSeperator}{resourceType.Name}{sResourceWrapperSuffix}";
        }
    }
}
