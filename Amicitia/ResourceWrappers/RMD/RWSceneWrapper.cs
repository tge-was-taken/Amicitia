using System;
using AtlusLibSharp.Graphics.RenderWare;
using System.Windows.Forms;
using System.IO;
using AtlusLibSharp.PS2.Graphics;

namespace Amicitia.ResourceWrappers
{
    internal class RWSceneWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.RWScene, SupportedFileType.DAEFile
        };

        public RWSceneWrapper(string text, RWScene scene) : base(text, scene) { }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.RWScene; }
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

        protected internal new RWScene WrappedObject
        {
            get { return (RWScene)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool CanRename
        {
            get { return false; }
        }

        protected internal override bool CanDelete
        {
            get { return false; }
        }

        protected internal override bool CanAdd
        {
            get { return true; }
        }

        public override void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Parent != null ? Path.GetFileNameWithoutExtension(Parent.Parent.Text) + ".dff" : Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RWScene:
                        WrappedObject = (RWScene)RWNode.Load(openFileDlg.FileName);
                        break;
                }

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        public override void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Parent != null ? Path.GetFileNameWithoutExtension(Parent.Parent.Text) + ".dff" : Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(FileFilterTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild wrapped object before export
                RebuildWrappedObject();

                switch (FileFilterTypes[saveFileDlg.FilterIndex - 1])
                {
                    case SupportedFileType.RWScene:
                        WrappedObject.Save(saveFileDlg.FileName);
                        break;

                    case SupportedFileType.DAEFile:
                        {
                            Assimp.AssimpContext ctx = new Assimp.AssimpContext();
                            ctx.ExportFile(RWScene.ToAssimpScene(WrappedObject), saveFileDlg.FileName, "collada");
                            break;
                        }
                }
            }
        }

        protected internal override void RebuildWrappedObject()
        {
        }

        protected internal override void InitializeWrapper()
        {
            if (IsInitialized)
            {
                MainForm.Instance.UpdateReferences();
            }
            else
            {
                IsInitialized = true;
            }
        }
    }
}
