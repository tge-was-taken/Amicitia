using System;
using AtlusLibSharp.Graphics.RenderWare;
using System.ComponentModel;
using System.Collections.Generic;

namespace Amicitia.ResourceWrappers
{
    internal class RWSceneWrapper : ResourceWrapper
    {
        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWScene, SupportedFileType.DAEFile
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWScene, (res, path) =>
                res.WrappedObject = RWNode.Load(path)
            },
            {
                SupportedFileType.DAEFile, ImportDAEFile 
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RWScene, (res, path) =>
                (res as RWSceneWrapper).WrappedObject.Save(path)
            },
            {
                SupportedFileType.DAEFile, ExportDAEFile
            }
        };

        private static void ImportDAEFile(ResourceWrapper res, string path)
        {
            var ctx = new Assimp.AssimpContext();
            res.WrappedObject = new RWScene(ctx.ImportFile(path));
        }

        private static void ExportDAEFile(ResourceWrapper res, string path)
        {
            var scene = (res as RWSceneWrapper).WrappedObject;
            var ctx = new Assimp.AssimpContext();
            ctx.ExportFile(RWScene.ToAssimpScene(scene), path, "collada");
        }

        private static void ExportOBJFile(ResourceWrapper res, string path)
        {
            var scene = (res as RWSceneWrapper).WrappedObject;
            var ctx = new Assimp.AssimpContext();
            ctx.ExportFile(RWScene.ToAssimpScene(scene), path, "obj");
        }

        /************************************/
        /* Import / export method overrides */
        /************************************/
        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetImportDelegates()
        {
            return ImportDelegates;
        }

        protected override Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetExportDelegates()
        {
            return ExportDelegates;
        }

        protected override SupportedFileType[] GetSupportedFileTypes()
        {
            return FileFilterTypes;
        }

        /***************/
        /* Constructor */
        /***************/
        public RWSceneWrapper(string text, RWScene scene) : base(text, scene, SupportedFileType.RWScene, true)
        {
            m_isModel = true;
            m_canRename = false;
            m_canAdd = true;
            InitializeContextMenuStrip();
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new RWScene WrappedObject
        {
            get
            {
                return (RWScene)m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        public int NodeCount
        {
            get { return WrappedObject.Nodes.Count; }
        }

        public int MeshCount
        {
            get { return WrappedObject.MeshCount; }
        }

        public int DrawCallCount
        {
            get { return WrappedObject.DrawCallCount; }
        }
    }
}
