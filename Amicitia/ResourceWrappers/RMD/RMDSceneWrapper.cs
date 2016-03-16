namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Graphics.RenderWare;
    using System.Windows.Forms;
    using Utilities;
    using System;
    using AtlusLibSharp.IO;
    using System.IO;

    internal class RMDSceneWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RMDScene
        };

        public SupportedFileType FileType
        {
            get { return SupportedFileType.RMDScene; }
        }

        protected internal new RMDScene WrappedObject
        {
            get { return (RMDScene)base.WrappedObject; }

            set { base.WrappedObject = value; }
        }

        protected internal RWTextureDictionaryWrapper TextureDictionaryWrapper { get; set; }

        protected internal TreeNode SceneListNode { get; set; }

        protected internal TreeNode FrameLinkListNode { get; set; }

        protected internal TreeNode AnimationSetListNode { get; set; }

        protected internal TreeNode MiscNodeListNode { get; set; }

        protected internal override bool IsModelResource
        {
            get { return true; }
        }

        public RMDSceneWrapper(string text, RMDScene rmd)
            : base(text, rmd)
        {
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RMDScene:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;
                }
            }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RMDScene:
                        WrappedObject = new RMDScene(openFileDlg.FileName);
                        break;
                }

                // re-init wrapper
                InitializeWrapper();
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject.Clear();

            TextureDictionaryWrapper.RebuildWrappedObject();
            WrappedObject.TextureDictionary = TextureDictionaryWrapper.WrappedObject as RWTextureDictionary;

            foreach (RWSceneWrapper wrapper in SceneListNode.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.Scenes.Add(wrapper.WrappedObject);
            }

            foreach (ResourceWrapper wrapper in FrameLinkListNode.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.FrameLinks.Add(
                    new RMDNodeLink((wrapper.WrappedObject as GenericBinaryFile).GetBytes()));
            }

            foreach (ResourceWrapper wrapper in AnimationSetListNode.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.AnimationSets.Add(
                    RWNode.Load((wrapper.WrappedObject as GenericBinaryFile).GetBytes()) as RMDAnimationSet);
            }

            foreach (ResourceWrapper wrapper in MiscNodeListNode.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.MiscNodes.Add(
                    RWNode.Load((wrapper.WrappedObject as GenericBinaryFile).GetBytes()));
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();
            TextureDictionaryWrapper = new RWTextureDictionaryWrapper("Textures", WrappedObject.TextureDictionary ?? new RWTextureDictionary());
            SceneListNode = new TreeNode("Scenes");
            FrameLinkListNode = new TreeNode("Frame Links");
            AnimationSetListNode = new TreeNode("Animation sets");
            MiscNodeListNode = new TreeNode("Misc Nodes");

            int sceneIndex = 0;
            foreach (RWScene scene in WrappedObject.Scenes)
            {
                SceneListNode.Nodes.Add(
                    new RWSceneWrapper(
                        string.Format("Scene[{0}]", sceneIndex++), 
                        scene));
            }

            int frameLinkIndex = 0;
            foreach (RMDNodeLink frameLink in WrappedObject.FrameLinks)
            {
                FrameLinkListNode.Nodes.Add(
                    new ResourceWrapper(
                        string.Format("FrameLink[{0}]", (frameLinkIndex++).ToString("00")),
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
                        string.Format("{0}[{1}]", miscNode.Type.ToString(), miscNodeIndex++),
                        new GenericBinaryFile(miscNode.GetBytes())));
            }

            Nodes.Add(TextureDictionaryWrapper, SceneListNode, FrameLinkListNode, AnimationSetListNode, MiscNodeListNode);
        }
    }
}
