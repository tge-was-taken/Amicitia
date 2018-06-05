using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AmicitiaLibrary.IO;

namespace Amicitia.ResourceWrappers
{
    [Flags]
    public enum CommonContextMenuOptions
    {
        Export   = 1 << 1,
        Replace  = 1 << 2,
        Add      = 1 << 3,
        Move     = 1 << 4,
        Rename   = 1 << 5,
        Encode   = 1 << 6,
        Delete   = 1 << 7,
    }

    internal interface IResourceWrapper : INotifyPropertyChanged
    {
        string Text { get; }

        object Resource { get; }

        SupportedFileType FileType { get; }

        bool NeedsRebuild { get; set; }

        CommonContextMenuOptions CommonContextMenuOptions { get; set; }

        void MoveUp();

        void MoveDown();

        void Delete();

        void Rename();

        bool Export();

        bool Export(string path);

        bool Export(string path, SupportedFileType type);

        bool Replace();

        bool Replace( string path );

        bool Replace( string path, SupportedFileType type );

        bool Add();

        bool Add( string path );

        bool Add( string path, SupportedFileType type );

        byte[] GetResourceBytes();

        MemoryStream GetResourceMemoryStream();
    }

    public class ContextMenuAction
    {
        public string Name { get; }

        public EventHandler OnClick { get; }

        public Keys ShortcutKeys { get; }

        public ContextMenuAction( string name, EventHandler onClick, Keys shortCutKeys )
        {
            Name = name;
            OnClick = onClick;
            ShortcutKeys = shortCutKeys;
        }
    }

    public abstract partial class ResourceWrapper<TResource> : TreeNode, IResourceWrapper, IDisposable
    {
        public static readonly Action<string, ResourceWrapper<TResource>> DefaultFileAddAction = (path, wrapper) =>
        {
            var resWrap = ResourceWrapperFactory.GetResourceWrapper(path);
            wrapper.Nodes.Add((TreeNode)resWrap);
        };

        private TResource mResource;
        private bool mInitialized;
        private bool mNeedsRebuild;
        private Dictionary<SupportedFileType, Func<TResource, string, TResource>> mFileReplaceActions;
        private Dictionary<SupportedFileType, Action<TResource, string>> mFileExportActions;
        private Dictionary<SupportedFileType, Action<string, ResourceWrapper<TResource>>> mFileAddActions;
        private Func<ResourceWrapper<TResource>, TResource> mRebuildAction;
        private List<ContextMenuAction> mCustomActions;

        public event PropertyChangedEventHandler PropertyChanged;

        [Browsable(false)]
        public TResource Resource
        {
            get
            {
                RebuildResource();
                return mResource;
            }
            private set
            {
                SetField(ref mResource, value, true);

                if (mInitialized)
                    PopulateViewFully();
            }
        }

        object IResourceWrapper.Resource => Resource;

        [Category("Resource info")]
        [OrderedProperty]
        public SupportedFileType FileType { get; }

#if DEBUG
        [Category("Debug")]
        [OrderedProperty]
        [Browsable(true)]
#else
	    [Browsable(false)]
#endif
        public bool NeedsRebuild
        {
            get { return mNeedsRebuild; }
            set { mNeedsRebuild = value; }
        }


        [Browsable(false)]
        public CommonContextMenuOptions CommonContextMenuOptions { get; set; }

        protected ResourceWrapper(string text, TResource resource)
            : base(text)
        {
            Resource = resource;
            FileType = SupportedFileManager.GetSupportedFileType(typeof(TResource));

            Initialize();
            PopulateViewFully();
            mInitialized = true;
        }

        public byte[] GetResourceBytes()
        {
            return GetResourceMemoryStream().ToArray();
        }

        public MemoryStream GetResourceMemoryStream()
        {
            var info = SupportedFileManager.GetSupportedFileInfo( typeof( TResource ) );
            return info.GetStream( Resource );
        }


        /// <summary>
        /// This method of base class is overriden so we can make the parent dirty because removing this node would change the parent's node collection.
        /// </summary>
        public new void Remove()
        {
            Dispose();
            SetRebuildFlag(Parent);
            base.Remove();
        }

        public void MoveUp()
        {
            TreeNode parent = Parent;

            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(this);
                if (index > 0)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index - 1, this);
                    SetRebuildFlag(Parent); // we modified the parent's nodes
                }
            }
            else if (TreeView.Nodes.Contains(this)) //root node
            {
                int index = TreeView.Nodes.IndexOf(this);
                if (index > 0)
                {
                    TreeView.Nodes.RemoveAt(index);
                    TreeView.Nodes.Insert(index - 1, this);
                    SetRebuildFlag(Parent); // we modified the parent's nodes
                }
            }

            TreeView.SelectedNode = this;
        }

        public void MoveDown()
        {
            TreeNode parent = Parent;
            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(this);
                if (index < parent.Nodes.Count - 1)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index + 1, this);
                    SetRebuildFlag(Parent);
                }
            }
            else if (TreeView != null && TreeView.Nodes.Contains(this)) //root node
            {
                int index = TreeView.Nodes.IndexOf(this);
                if (index < TreeView.Nodes.Count - 1)
                {
                    TreeView.Nodes.RemoveAt(index);
                    TreeView.Nodes.Insert(index + 1, this);
                    SetRebuildFlag(Parent);
                }
            }

            TreeView.SelectedNode = this;
        }

        public void Delete()
        {
            Dispose();
            Remove();
        }

        public void Rename()
        {
            TreeView.LabelEdit = true;
            BeginEdit(); // EndEdit() is called in the TreeView AfterLabelEdit event
            NeedsRebuild = true;
        }

        public bool Export()
        {
            if (mFileExportActions == null)
                return false;

            using (var saveFileDlg = new SaveFileDialog())
            {
                var fileTypes = mFileExportActions.Keys.ToArray();
            
                saveFileDlg.AutoUpgradeEnabled = true;
                saveFileDlg.CheckPathExists = true;
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileManager.GetFilteredFileFilter(fileTypes);
                saveFileDlg.OverwritePrompt = true;
                saveFileDlg.Title = "Select a file to export to";
                saveFileDlg.ValidateNames = true;

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                var fileType = GetCorrectedSupportedFileType( saveFileDlg.FilterIndex - 1, saveFileDlg.FileName, fileTypes );
                return Export(saveFileDlg.FileName, fileType);
            }
        }

        public bool Export(string path)
        {
            var fileInfo = SupportedFileManager.GetSupportedFileInfo( Path.GetExtension( path ) );
            return Export(path, fileInfo.EnumType);
        }

        public bool Export(string path, SupportedFileType type)
        {
            if (mFileExportActions == null)
                return false;

            var fileExportAction = mFileExportActions[type];
            fileExportAction.Invoke(Resource, path);
            return true;
        }

        public bool Replace()
        {
            if (mFileReplaceActions == null)
                return false;

            using (var openFileDlg = new OpenFileDialog())
            {
                var fileTypes = mFileReplaceActions.Keys.ToArray();

                openFileDlg.AutoUpgradeEnabled = true;
                openFileDlg.CheckPathExists = true;
                openFileDlg.CheckFileExists = true;
                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileManager.GetFilteredFileFilter(fileTypes);
                openFileDlg.Multiselect = false;
                openFileDlg.SupportMultiDottedExtensions = true;
                openFileDlg.Title = "Select a replacement file";
                openFileDlg.ValidateNames = true;

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                var fileType = GetCorrectedSupportedFileType(openFileDlg.FilterIndex - 1, openFileDlg.FileName, fileTypes);
                return Replace(openFileDlg.FileName, fileType);
            }
        }

        public bool Replace(string path)
        {
            var fileInfo = SupportedFileManager.GetSupportedFileInfo( Path.GetExtension( path ) );
            return Replace(path, fileInfo.EnumType);
        }

        public bool Replace(string path, SupportedFileType type)
        {
            if (mFileReplaceActions == null)
                return false;

            var fileReplaceAction = mFileReplaceActions[type];
            var res = Resource;
            var newRes = fileReplaceAction.Invoke( res, path);

            if ( !newRes.Equals(res) && res is IDisposable disposable )
                disposable.Dispose();

            Resource = newRes;

            return true;
        }

        public void AddNode( TreeNode node )
        {
            SetRebuildFlag( this );
            Nodes.Add( node );
        }

        public bool Add()
        {
            if (mFileAddActions == null)
                return false;

            bool hasAdded = false;

            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                var fileTypes = mFileAddActions.Keys.ToArray();

                openFileDlg.AutoUpgradeEnabled = true;
                openFileDlg.CheckPathExists = true;
                openFileDlg.CheckFileExists = true;
                openFileDlg.Filter = SupportedFileManager.GetFilteredFileFilter(fileTypes);
                openFileDlg.Multiselect = true;
                openFileDlg.SupportMultiDottedExtensions = true;
                openFileDlg.Title = "Select file(s) to add";
                openFileDlg.ValidateNames = true;

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                foreach (string fileName in openFileDlg.FileNames)
                {
                    var fileType = GetCorrectedSupportedFileType( openFileDlg.FilterIndex - 1, fileName, fileTypes );
                    hasAdded |= Add( fileName, fileType );
                }          
            }

            return hasAdded;
        }

        public bool Add(string path)
        {
            var fileInfo = SupportedFileManager.GetSupportedFileInfo( Path.GetExtension( path ) );
            return Add(path, fileInfo.EnumType);
        }

        public bool Add(string path, SupportedFileType type)
        {
            var fileAddAction = mFileAddActions[type];
            fileAddAction.Invoke(path, this);

            NeedsRebuild = true;
            return true;
        }

        protected void MoveUpEventHandler(object sender, EventArgs e)
        {
            MoveUp();
        }

        protected void MoveDownEventHandler(object sender, EventArgs e)
        {
            MoveDown();
        }

        protected void DeleteEventHandler(object sender, EventArgs e)
        {
            Delete();
        }

        protected void RenameEventHandler(object sender, EventArgs e)
        {
            Rename();
        }

        protected void ExportEventHandler(object sender, EventArgs e)
        {
            Export();
        }

        protected void ReplaceEventHandler(object sender, EventArgs e)
        {
            Replace();
        }

        protected void AddEventHandler(object sender, EventArgs e)
        {
            Add();
        }

        protected abstract void Initialize();

        protected abstract void PopulateView();

        protected void RegisterFileReplaceAction(SupportedFileType type, Func<TResource, string, TResource> action)
        {
            if (mFileReplaceActions == null)
                mFileReplaceActions = new Dictionary<SupportedFileType, Func<TResource, string, TResource>>();
            mFileReplaceActions[type] = action;
        }

        protected void RegisterFileExportAction(SupportedFileType type, Action<TResource, string> action)
        {
            if (mFileExportActions == null)
                mFileExportActions = new Dictionary<SupportedFileType, Action<TResource, string>>();
            mFileExportActions[type] = action;
        }

        protected void RegisterFileAddAction(SupportedFileType type, Action<string, ResourceWrapper<TResource>> action)
        {
            if (mFileAddActions == null)
                mFileAddActions = new Dictionary<SupportedFileType, Action<string, ResourceWrapper<TResource>>>();
            mFileAddActions[type] = action;
        }

        protected void RegisterRebuildAction(Func<ResourceWrapper<TResource>, TResource> action)
        {
            mRebuildAction = action;
        }

        protected void RegisterCustomAction( string name, Keys shortcutKeys, EventHandler onClick )
        {
            if ( mCustomActions == null )
                mCustomActions = new List<ContextMenuAction>();

            mCustomActions.Add( new ContextMenuAction(name, onClick, shortcutKeys) );
        }

        protected void SetField<T>(ref T field, T value, bool onlyParentNeedsRebuild = false, [CallerMemberName] string propertyName = null)
        {
            if (typeof(T).IsValueType)
            {
                if (Equals(field, value))
                    return;
            }

            field = value;

            if (!mInitialized)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            mNeedsRebuild = !onlyParentNeedsRebuild;

            SetRebuildFlag(Parent);
        }

        protected void SetProperty<T>(object instance, T value, bool onlyParentNeedsRebuild = false, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var prop = instance.GetType().GetProperty(propertyName);
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            if (typeof(T).IsValueType)
            {
                if (Equals(prop.GetValue(instance, null), value))
                    return;
            }

            prop.SetValue(instance, value);

            if (!mInitialized)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (!onlyParentNeedsRebuild)
                mNeedsRebuild = true;

            SetRebuildFlag(Parent);
        }

        private static void SetRebuildFlag(TreeNode node)
        {
            while (true)
            {
                if (node == null)
                    return;

                if (node is IResourceWrapper wrapper)
                {
                    wrapper.NeedsRebuild = true;
                }

                node = node.Parent;
            }
        }

        private static SupportedFileType GetCorrectedSupportedFileType(int fileTypeIndex, string fileName, IList<SupportedFileType> fileTypes)
        {
            var fileType = fileTypes[fileTypeIndex];

            // check if the selected type in the filter contains the extension of the path
            // if it doesn't, try to find the first best matching type
            var extension = Path.GetExtension( fileName );
            if (extension != null)
            {
                var info = SupportedFileManager.GetSupportedFileInfo(fileType);

                if (!info.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase))
                {
                    var actualInfo = SupportedFileManager.SupportedFileInfos
                        .Where(x => fileTypes.Contains(x.EnumType))
                        .FirstOrDefault(
                            x => x.Extensions.Contains(extension, StringComparer.InvariantCultureIgnoreCase));

                    if (actualInfo != null)
                    {
                        fileType = actualInfo.EnumType;
                    }
                }
            }

            return fileType;
        }

        /// <summary>
        /// Clears nodes and populates the view and context menu strip.
        /// </summary>
        private void PopulateViewFully()
        {
            if (mInitialized)
                Nodes.Clear();

            PopulateView();
            PopulateContextMenuStrip();
        }

        /// <summary>
        /// Populates the context menu strip, should be called after the context menu options have been set.
        /// </summary>
        private void PopulateContextMenuStrip()
        {
            ContextMenuStrip = new ContextMenuStrip();

            if ( mCustomActions != null )
            {
                foreach ( var item in mCustomActions )
                {
                    ContextMenuStrip.Items.Add( new ToolStripMenuItem( item.Name, null, item.OnClick, item.ShortcutKeys ) );
                }

                ContextMenuStrip.Items.Add( new ToolStripSeparator() );
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Export))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Export", null, ExportEventHandler, Keys.Control | Keys.E));
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Replace))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Replace", null, ReplaceEventHandler, Keys.Control | Keys.R));
                if (!CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Add))
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Add))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Add", null, AddEventHandler, Keys.Control | Keys.A));
                if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Move))
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Move))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUpEventHandler, Keys.Control | Keys.Up));
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Down", null, MoveDownEventHandler, Keys.Control | Keys.Down));
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Rename))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Re&name", null, RenameEventHandler, Keys.Control | Keys.N));
                if (!CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Encode) && CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Delete))
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Encode))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Encode", null, null, Keys.Control | Keys.N));
                if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Delete))
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CommonContextMenuOptions.HasFlag(CommonContextMenuOptions.Delete))
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Delete", null, DeleteEventHandler, Keys.Control | Keys.Delete));
            }
        }

        /// <summary>
        /// Wrapper around the rebuild action. Checks for null and reinitializes the wrapper afterwards.
        /// </summary>
        private void RebuildResource()
        {
            if (!NeedsRebuild)
                return;

            mNeedsRebuild = false;

            if (mRebuildAction != null)
            {
                Resource = mRebuildAction.Invoke(this);
                SetRebuildFlag(Parent);
            }
        }

        public virtual void Dispose()
        {
            if ( Resource is IDisposable disposable )
                disposable.Dispose();
        }
    }

    public class BinaryBaseWrapper : ResourceWrapper<BinaryBase>
    {
        public BinaryBaseWrapper(string text, BinaryBase resource) : base(text, resource)
        {
        }

        protected override void Initialize()
        {
            CommonContextMenuOptions = CommonContextMenuOptions.Export | CommonContextMenuOptions.Replace | CommonContextMenuOptions.Move |
                                       CommonContextMenuOptions.Rename | CommonContextMenuOptions.Delete;

            RegisterFileExportAction(SupportedFileType.Resource, (res, path) => res.Save(path));
            RegisterFileReplaceAction(SupportedFileType.Resource, (res, path) => new BinaryFile(path));
        }

        protected override void PopulateView()
        {
        }
    }

    public class BinaryFileWrapper : BinaryBaseWrapper
    {
        public BinaryFileWrapper( string text, BinaryFile resource ) : base( text, resource )
        {
        }
    }
}
