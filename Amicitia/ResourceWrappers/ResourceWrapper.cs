namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Collections.Generic;
    using AtlusLibSharp.IO;

    internal partial class ResourceWrapper : TreeNode, INotifyPropertyChanged
    {
        /*******************/
        /* Private members */
        /*******************/

        // wrapped object storage
        protected internal object m_wrappedObject;
        protected internal SupportedFileType m_wrappedFileType;

        // wrapper states
        protected internal bool m_isInitialized = false;

        // wrapped object states
        protected internal bool m_isDirty = false;
        protected internal bool m_isImage = false;
        protected internal bool m_isModel = false;

        // context menu states
        protected internal bool m_canExport = true;
        protected internal bool m_canReplace = true;
        protected internal bool m_canMove = true;
        protected internal bool m_canRename = true;
        protected internal bool m_canDelete = true;
        protected internal bool m_canAdd = false;
        protected internal bool m_canEncode = false;

        /*********************/
        /* File filter types */
        /*********************/
        public static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
           SupportedFileType.Resource
        };

        /*****************************************/
        /* Import / Export delegate dictionaries */
        /*****************************************/
        public static readonly Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ImportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            { SupportedFileType.Resource, (x, y) => x.WrappedObject = new GenericBinaryFile(y) }
        };

        public static readonly Dictionary<SupportedFileType, Action<ResourceWrapper, string>> ExportDelegates = new Dictionary<SupportedFileType, Action<ResourceWrapper, string>>()
        {
            { SupportedFileType.Resource, (x, y) => (x.WrappedObject as BinaryFileBase).Save(y) }
        };

        /***********************************/
        /* Import / export virtual methods */
        /***********************************/
        protected virtual Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetImportDelegates()
        {
            return ImportDelegates;
        }

        protected virtual Dictionary<SupportedFileType, Action<ResourceWrapper, string>> GetExportDelegates()
        {
            return ExportDelegates;
        }

        protected virtual SupportedFileType[] GetSupportedFileTypes()
        {
            return FileFilterTypes;
        }

        /// <summary>
        /// Fired when a property in this class changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new generic resource wrapper.
        /// </summary>
        /// <param name="text">Node text to display on the tree view.</param>
        /// <param name="wrappedObject">The object to store in the wrapper.</param>
        /// <param name="type">The <see cref="SupportedFileType"/> of the wrapped object.</param>
        /// <param name="suppressContextMenuInit">Sets if the call to initialize the context menu strip will be supressed (use if context menu states are set in the constructor).</param>
        public ResourceWrapper(string text, object wrappedObject, SupportedFileType type, bool suppressContextMenuInit)
            : base(text)
        {
            m_wrappedFileType = type;
            m_wrappedObject = wrappedObject;
       
            if (!suppressContextMenuInit)
                InitializeContextMenuStrip();

            InitializeWrapper();
        }

        /// <summary>
        /// Creates a new generic resource wrapper.
        /// </summary>
        /// <param name="text">Node text to display on the tree view.</param>
        /// <param name="wrappedObject">The object to store in the wrapper.</param>
        public ResourceWrapper(string text, object wrappedObject)
            : this(text, wrappedObject, SupportedFileType.Resource, false)
        {
        }

        /***************************/
        /* Wrapper node properties */
        /***************************/

        /// <summary>
        /// Gets or sets the wrapped object.
        /// </summary>
        [Browsable(false)]
        public virtual object WrappedObject
        {
            get
            {
                return m_wrappedObject;
            }
            set
            {
                SetProperty(ref m_wrappedObject, value);
            }
        }

        /// <summary>
        /// Gets if the wrapper node has been initialized.
        /// </summary>
        [Browsable(false)]
        public bool IsInitialized
        {
            get { return m_isInitialized; }
        }

        /// <summary>
        /// Gets the <see cref="SupportedFileType"/> of the wrapped object.
        /// </summary>
        public SupportedFileType FileType
        {
            get { return m_wrappedFileType; }
        }

        /// <summary>
        /// Gets if the wrapped object was modified and needs to be rebuilt.
        /// </summary>
#if DEBUG
        [Browsable(true)]
#else
        [Browsable(false)]
#endif
        public bool IsDirty
        {
            get { return m_isDirty; }
            set { SetProperty(ref m_isDirty, value); }
        }

        /**********************/
        /* Context menu bools */
        /**********************/

        /// <summary>
        /// Gets if the Export context menu option is visible for this wrapper. Default is true.
        /// </summary>
        [Browsable(false)]
        public bool CanExport
        {
            get { return m_canExport; }
        }

        /// <summary>
        /// Gets if the Replace context menu option is visible for this wrapper. Default is true.
        /// </summary>
        [Browsable(false)]
        public bool CanReplace
        {
            get { return m_canReplace; }
        }

        /// <summary>
        /// Gets if the Move Up and Move Up context menu option are visible for this wrapper. Default is true.
        /// </summary>
        [Browsable(false)]
        public bool CanMove
        {
            get { return m_canMove; }
        }

        /// <summary>
        /// Gets if the Rename context menu option is visible for this wrapper. Default is true.
        /// </summary>
        [Browsable(false)]
        public bool CanRename
        {
            get { return m_canRename; }
        }

        /// <summary>
        /// Gets if the Delete context menu option is visible for this wrapper. Default is true.
        /// </summary>
        [Browsable(false)]
        public bool CanDelete
        {
            get { return m_canDelete; }
        }

        /// <summary>
        /// Gets if the Add context menu option is visible for this wrapper. Default is false.
        /// </summary>
        [Browsable(false)]
        public bool CanAdd
        {
            get { return m_canAdd; }
        }

        /// <summary>
        /// Gets if the Encode context menu option is visible for this wrapper. Default is false.
        /// </summary>
        [Browsable(false)]
        public bool CanEncode
        {
            get { return m_canEncode; }
        }

        /// <summary>
        /// Gets if this wrapped object implements the <see cref="ITextureFile"/> interface. Default is false.
        /// </summary>
        [Browsable(false)]
        public bool IsImageResource
        {
            get { return m_isImage; }
        }

        /// <summary>
        /// Gets if this wrapped object is a model resource. Default is false.
        /// </summary>
        [Browsable(false)]
        public bool IsModelResource
        {
            get { return m_isModel; }
        }

        /************************/
        /* Context menu actions */
        /************************/

        /// <summary>
        /// Move the wrapper node up in the tree (if possible).
        /// </summary>
        public void MoveUp(object sender, EventArgs e)
        {
            TreeNode parent = Parent;
            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(this);
                if (index > 0)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index - 1, this);
                }
            }
            else if (TreeView.Nodes.Contains(this)) //root node
            {
                int index = TreeView.Nodes.IndexOf(this);
                if (index > 0)
                {
                    TreeView.Nodes.RemoveAt(index);
                    TreeView.Nodes.Insert(index - 1, this);
                }
            }
            TreeView.SelectedNode = this;
        }

        /// <summary>
        /// Move the wrapper node down in the tree (if possible).
        /// </summary>
        public void MoveDown(object sender, EventArgs e)
        {
            TreeNode parent = Parent;
            if (parent != null)
            {
                int index = parent.Nodes.IndexOf(this);
                if (index < parent.Nodes.Count - 1)
                {
                    parent.Nodes.RemoveAt(index);
                    parent.Nodes.Insert(index + 1, this);
                }
            }
            else if (TreeView != null && TreeView.Nodes.Contains(this)) //root node
            {
                int index = TreeView.Nodes.IndexOf(this);
                if (index < TreeView.Nodes.Count - 1)
                {
                    TreeView.Nodes.RemoveAt(index);
                    TreeView.Nodes.Insert(index + 1, this);
                }
            }
            TreeView.SelectedNode = this;
        }

        /// <summary>
        /// Deletes the wrapper node from the tree.
        /// </summary>
        public void Delete(object sender, EventArgs e)
        {
            Remove();
        }

        /// <summary>
        /// Renames the wrapper node in the tree.
        /// </summary>
        public void Rename(object sender, EventArgs e)
        {
            TreeView.LabelEdit = true;
            BeginEdit(); // EndEdit() is called in the TreeView AfterLabelEdit event
        }

        /// <summary>
        /// Opens up the file export dialog for exporting this wrapper node.
        /// </summary>
        public void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                var delegates = GetExportDelegates();
                var fileTypes = GetSupportedFileTypes();

                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(fileTypes);

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild if dirty before export
                if (m_isDirty)
                {
                    RebuildWrappedObject();
                }

                delegates[fileTypes[saveFileDlg.FilterIndex - 1]].Invoke(this, saveFileDlg.FileName);
            }
        }

        /// <summary>
        /// Opens up the file open dialog and replaces the wrapper node with the opened file.
        /// </summary>
        public void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                var delegates = GetImportDelegates();
                var fileTypes = GetSupportedFileTypes();

                openFileDlg.FileName = Text;
                openFileDlg.Filter = SupportedFileHandler.GetFilteredFileFilter(fileTypes);

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // get the delegate
                delegates[fileTypes[openFileDlg.FilterIndex-1]].Invoke(this, openFileDlg.FileName);

                // re-init
                InitializeWrapper();

                // set it to not dirty, the delegate will have invoked the property change for the wrapped object
                // so the parents have already been informed about the replacement
                m_isDirty = false;
            }
        }

        /// <summary>
        /// Opens up the file open dialog and adds the opened file to the wrapper node's collection.
        /// </summary>
        public virtual void Add(object sender, EventArgs e) { }

        /// <summary>
        /// Opens up the encoder dialog for this wrapper node.
        /// </summary>
        public virtual void Encode(object sender, EventArgs e) { }

        /// <summary>
        /// Rebuilds the data of the wrapped object.
        /// </summary>
        internal virtual void RebuildWrappedObject()
        {
            m_isDirty = false;
        }

        /// <summary>
        /// Initializes the wrapper.
        /// </summary>
        internal virtual void InitializeWrapper()
        {
            if (m_isInitialized)
            {
                MainForm.Instance.UpdateReferences();
            }
            else
            {
                m_isInitialized = true;
            }
        }

        /***********************/
        /* Convienence methods */
        /***********************/

        /// <summary>
        /// Gets the data of the wrapped object in a byte array.
        /// </summary>
        public byte[] GetBytes()
        {
            return (WrappedObject as BinaryFileBase).GetBytes();
        }

        /// <summary>
        /// Gets the data of the wrapped object in a memory stream.
        /// </summary>
        public MemoryStream GetMemoryStream()
        {
            return (WrappedObject as BinaryFileBase).GetMemoryStream();
        }

        /**********************************/
        /* Property get/set event methods */
        /**********************************/

        /// <summary>
        /// Triggers the property changed event for a specific property.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // everything's alll dirty now
            m_isDirty = true;
            SetParentsToDirty(this);
        }

        private void SetParentsToDirty(TreeNode node)
        {
            if (node.Parent == null)
                return;

            if (node.Parent is ResourceWrapper)
                (node.Parent as ResourceWrapper).m_isDirty = true; // don't use the property as it's unnecessary

            SetParentsToDirty(node.Parent);
        }

        /// <summary>
        /// Sets a property if it is not equal to the current value and fires the <see cref="OnPropertyChanged"/> event.
        /// </summary>
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(property, value))
                return;

            property = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets a property if it is not equal to the current value and fires the <see cref="OnPropertyChanged"/> event.
        /// </summary>
        protected void SetProperty<T>(object instance, T value, [CallerMemberName] string propertyName = null)
        {
            var prop = instance.GetType().GetProperty(propertyName);

            if (Equals(prop.GetValue(instance, null), value))
                return;

            prop.SetValue(instance, value);
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Initializes the context menu strip using the boolean context menu properties.
        /// </summary>
        protected internal void InitializeContextMenuStrip()
        {
            ContextMenuStrip = new ContextMenuStrip();

            if (m_canExport)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Export", null, Export, Keys.Control | Keys.E));
            }

            if (m_canReplace)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Replace", null, Replace, Keys.Control | Keys.R));
                if (!m_canAdd)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (m_canAdd)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Add", null, Add, Keys.Control | Keys.A));
                if (m_canMove)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (m_canMove)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUp, Keys.Control | Keys.Up));
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Down", null, MoveDown, Keys.Control | Keys.Down));
            }

            if (m_canRename)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Re&name", null, Rename, Keys.Control | Keys.N));
                if (!m_canEncode && m_canDelete)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (m_canEncode)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Encode", null, Encode, Keys.Control | Keys.N));
                if (m_canDelete)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (m_canDelete)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Delete", null, Delete, Keys.Control | Keys.Delete));
            }
        }
    }
}
