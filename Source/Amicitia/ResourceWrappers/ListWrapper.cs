namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp.SMT3.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.Reflection;

    internal class ListWrapper<ResourceT> : ResourceWrapper 
    {
        public Dictionary<Type, Type> resToWrapperType = new Dictionary<Type, Type>()
        {
            { typeof(SPRKeyFrame), typeof(SPRKeyFrameWrapper) }
        };

        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.Default
        };

        public ListWrapper(string text, List<ResourceT> list)
            : base(text, list)
        {

        }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.Default; }
        }

        public int EntryCount
        {
            get { return Nodes.Count; }
        }

        protected internal new List<ResourceT> WrappedObject
        {
            get { return (List<ResourceT>)base.WrappedObject; }
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
                openFileDlg.FileName = Text;
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

                //switch (FileFilterTypes[openFileDlg.FilterIndex - 1])
                //{
                //}

                // re-init the wrapper
                InitializeWrapper();
            }
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

                // rebuild wrapped object before export
                RebuildWrappedObject();

                //switch (FileFilterTypes[saveFileDlg.FilterIndex - 1])
                //{
                //}
            }
        }

        public override void Add(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(RWTextureNativeWrapper.FileFilterTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(openFileDlg.FileName);

                if (supportedFileIndex == -1)
                {
                    return;
                }

                try
                {
                    //switch (RWTextureNativeWrapper.FileFilterTypes[openFileDlg.FilterIndex - 1])
                    //{
                    //}
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error occured.");
                }

                // re-init the wrapper
                InitializeWrapper();
            }
        }

        protected internal override void RebuildWrappedObject()
        {
            WrappedObject.Clear();
            for (int i = 0; i < Nodes.Count; i++)
            {
                // rebuild the data
                ResourceWrapper node = (ResourceWrapper)Nodes[i];
                node.RebuildWrappedObject();

                // set the wrapped object
                WrappedObject.Add((ResourceT)node.WrappedObject);
            }
        }

        protected internal override void InitializeWrapper()
        {
            Nodes.Clear();

            int idx = 0;
            foreach (ResourceT node in WrappedObject)
            {
                Nodes.Add((ResourceWrapper)Activator.CreateInstance(resToWrapperType[typeof(ResourceT)], new object[] { $"KeyFrame[{idx++}]", node }));
            }

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
