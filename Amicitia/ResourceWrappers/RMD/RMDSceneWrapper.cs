namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.Persona3.RenderWare;
    using System.Windows.Forms;
    using Utilities;
    using System;

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

        protected internal TreeNode ClumpListWrapper { get; set; }

        protected internal TreeNode FrameLinkListWrapper { get; set; }

        protected internal TreeNode AnimationSetListWrapper { get; set; }

        protected internal TreeNode MiscNodeListWrapper { get; set; }

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

            foreach (ResourceWrapper wrapper in ClumpListWrapper.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.Clumps.Add(wrapper.WrappedObject as RWClump);
            }

            foreach (ResourceWrapper wrapper in FrameLinkListWrapper.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.FrameLinks.Add(wrapper.WrappedObject as RMDFrameLink);
            }

            foreach (ResourceWrapper wrapper in AnimationSetListWrapper.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.AnimationSets.Add(wrapper.WrappedObject as RMDAnimationSet);
            }

            foreach (ResourceWrapper wrapper in MiscNodeListWrapper.Nodes)
            {
                wrapper.RebuildWrappedObject();
                WrappedObject.MiscNodes.Add(wrapper.WrappedObject as RWNode);
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();
            TextureDictionaryWrapper = new RWTextureDictionaryWrapper("Textures", WrappedObject.TextureDictionary ?? new RWTextureDictionary());
            ClumpListWrapper = new TreeNode("Models");
            FrameLinkListWrapper = new TreeNode("Frame Links");
            AnimationSetListWrapper = new TreeNode("Animation sets");
            MiscNodeListWrapper = new TreeNode("Misc Nodes");

            int clumpIndex = 0;
            foreach (RWClump clump in WrappedObject.Clumps)
            {
                ClumpListWrapper.Nodes.Add(new ResourceWrapper("Model[" + clumpIndex++ + "]", clump));
            }

            int frameLinkIndex = 0;
            foreach (RMDFrameLink frameLink in WrappedObject.FrameLinks)
            {
                FrameLinkListWrapper.Nodes.Add(new ResourceWrapper("FrameLink[" + frameLinkIndex++ + "]", frameLink));
            }

            int animSetIndex = 0;
            foreach (RMDAnimationSet animSet in WrappedObject.AnimationSets)
            {
                AnimationSetListWrapper.Nodes.Add(new ResourceWrapper("AnimationSet[" + (animSetIndex++).ToString("00") + "]", animSet));
            }

            int miscNodeIndex = 0;
            foreach (RWNode miscNode in WrappedObject.MiscNodes)
            {
                MiscNodeListWrapper.Nodes.Add(new ResourceWrapper(miscNode.Type.ToString() + "[" + miscNodeIndex++ + "]", miscNode));
            }

            Nodes.Add(TextureDictionaryWrapper, ClumpListWrapper, FrameLinkListWrapper, AnimationSetListWrapper, MiscNodeListWrapper);
        }
    }
}
