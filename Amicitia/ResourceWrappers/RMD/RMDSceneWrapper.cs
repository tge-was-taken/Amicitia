namespace Amicitia.ResourceWrappers
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using AtlusLibSharp.IO;
    using AtlusLibSharp.Graphics.RenderWare;
    using Utilities;

    internal class RMDSceneWrapper : ResourceWrapper
    {
        /*******************/
        /* Private members */
        /*******************/
        private RWTextureDictionaryWrapper m_texturesNode;
        private TreeNode m_sceneListNode;
        private TreeNode m_nodeLinkListNode;
        private TreeNode m_animSetListNode;
        private TreeNode m_miscNodeListNode;

        /*********************/
        /* File filter types */
        /*********************/
        public static readonly new SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RMDScene
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RMDScene, (res, path) =>
                res.WrappedObject = new RMDScene(path)
            }
        };

        public static readonly new Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            {
                SupportedFileType.RMDScene, (res, path) =>
                (res as RMDSceneWrapper).WrappedObject.Save(path)
            },
        };

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
        public RMDSceneWrapper(string text, RMDScene rmd)
            : base(text, rmd, SupportedFileType.RMDScene, false)
        {
            m_isModel = true;
        }

        /*****************************/
        /* Wrapped object properties */
        /*****************************/
        [Browsable(false)]
        public new RMDScene WrappedObject
        {
            get
            {
                return (RMDScene)m_wrappedObject; }

            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        [Browsable(false)]
        public RWTextureDictionaryWrapper TexturesNode
        {
            get { return m_texturesNode; }
            set
            {
                SetProperty(ref m_texturesNode, value);
            }
        }

        [Browsable(false)]
        public TreeNode SceneListNode
        {
            get { return m_sceneListNode; }
            set
            {
                SetProperty(ref m_sceneListNode, value);
            }
        }

        [Browsable(false)]
        public TreeNode NodeLinkListNode
        {
            get { return m_nodeLinkListNode; }
            set
            {
                SetProperty(ref m_nodeLinkListNode, value);
            }
        }

        [Browsable(false)]
        public TreeNode AnimationSetListNode
        {
            get { return m_animSetListNode; }
            set
            {
                SetProperty(ref m_animSetListNode, value);
            }
        }

        [Browsable(false)]
        public TreeNode MiscNodeListNode
        {
            get { return m_miscNodeListNode; }
            set
            {
                SetProperty(ref m_miscNodeListNode, value);
            }
        }

        /*********************************/
        /* Base wrapper method overrides */
        /*********************************/
        internal override void RebuildWrappedObject()
        {
            var scene = new RMDScene();

            if (TexturesNode.IsDirty)
                TexturesNode.RebuildWrappedObject();

            scene.TextureDictionary = TexturesNode.WrappedObject;

            foreach (RWSceneWrapper wrapper in SceneListNode.Nodes)
            {
                if (wrapper.IsDirty)
                    wrapper.RebuildWrappedObject();

                scene.Scenes.Add(wrapper.WrappedObject);
            }

            foreach (ResourceWrapper wrapper in NodeLinkListNode.Nodes)
            {
                if (wrapper.IsDirty)
                    wrapper.RebuildWrappedObject();

                scene.NodeLinks.Add(
                    new RMDNodeLink((wrapper.WrappedObject as GenericBinaryFile).GetBytes()));
            }

            foreach (ResourceWrapper wrapper in AnimationSetListNode.Nodes)
            {
                if (wrapper.IsDirty)
                    wrapper.RebuildWrappedObject();

                scene.AnimationSets.Add(
                    RWNode.Load((wrapper.WrappedObject as GenericBinaryFile).GetBytes()) as RMDAnimationSet);
            }

            foreach (ResourceWrapper wrapper in MiscNodeListNode.Nodes)
            {
                if (wrapper.IsDirty)
                    wrapper.RebuildWrappedObject();

                scene.MiscNodes.Add(
                    RWNode.Load((wrapper.WrappedObject as GenericBinaryFile).GetBytes()));
            }

            m_wrappedObject = scene;
            m_isDirty = false;
        }

        internal override void InitializeWrapper()
        {
            Nodes.Clear();
            TexturesNode = new RWTextureDictionaryWrapper("Textures", WrappedObject.TextureDictionary ?? new RWTextureDictionary());
            SceneListNode = new TreeNode("Scenes");
            NodeLinkListNode = new TreeNode("NodeLinks");
            AnimationSetListNode = new TreeNode("AnimationSets");
            MiscNodeListNode = new TreeNode("MiscData");

            int sceneIndex = 0;
            foreach (RWScene scene in WrappedObject.Scenes)
            {
                SceneListNode.Nodes.Add(
                    new RWSceneWrapper(
                        string.Format("SceneData[{0}]", sceneIndex++), 
                        scene));
            }

            int nodeLinkIndex = 0;
            foreach (RMDNodeLink frameLink in WrappedObject.NodeLinks)
            {
                NodeLinkListNode.Nodes.Add(
                    new ResourceWrapper(
                        string.Format("NodeLinkData[{0}]", (nodeLinkIndex++).ToString("00")),
                        new GenericBinaryFile(frameLink.GetBytes())));
            }

            int animSetIndex = 0;
            foreach (RMDAnimationSet animSet in WrappedObject.AnimationSets)
            {
                AnimationSetListNode.Nodes.Add(
                    new ResourceWrapper(
                        string.Format("AnimationSet[{0}]", (animSetIndex++).ToString("00")),
                        new GenericBinaryFile(animSet.GetBytes())));
            }

            int miscNodeIndex = 0;
            foreach (RWNode miscNode in WrappedObject.MiscNodes)
            {
                MiscNodeListNode.Nodes.Add(
                    new ResourceWrapper(
                        string.Format("MiscData[{0}][{1}]", miscNode.Type.ToString(), miscNodeIndex++),
                        new GenericBinaryFile(miscNode.GetBytes())));
            }

            Nodes.Add(TexturesNode, SceneListNode, NodeLinkListNode, AnimationSetListNode, MiscNodeListNode);
        }
    }
}
