namespace Amicitia.ResourceWrappers
{
    using System;
    using System.Windows.Forms;
    using System.ComponentModel;
    using AtlusLibSharp.IO;

    internal partial class ResourceWrapper : TreeNode, INotifyPropertyChanged
    {
        private object _wrappedObject;
        private bool _isInitialized;
        private ResourceProperty[] _resProperties;

        public ResourceWrapper(string text, object wrappedObject, params ResourceProperty[] properties)
            : base(text)
        {
            _wrappedObject = wrappedObject;
            _resProperties = properties;
            InitializeContextMenuStrip();
            InitializeWrapper();
        }

        // TODO: Actually implement PropertyChanged on the properties

        /// <summary>
        /// Fired when a property in this class changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers the property changed event for a specific property.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Common Wrapper properties

        protected internal virtual object WrappedObject
        {
            get { return _wrappedObject; }
            set { _wrappedObject = value; }
        }

        protected internal ResourceProperty[] ResourceProperties
        {
            get { return _resProperties; }
            set
            {
                _resProperties = value;
            }
        }

        protected internal bool IsInitialized
        {
            get { return _isInitialized; }
            set { _isInitialized = value; }
        }

        // Context menu bools

        /// <summary>
        /// Determines whether the Export context menu option is visible for this wrapper. Default is true.
        /// </summary>
        protected internal virtual bool CanExport
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the Replace context menu option is visible for this wrapper. Default is true.
        /// </summary>
        protected internal virtual bool CanReplace
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the Move Up / Move Up context menu option are visible for this wrapper. Default is true.
        /// </summary>
        protected internal virtual bool CanMove
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the Rename context menu option is visible for this wrapper. Default is true.
        /// </summary>
        protected internal virtual bool CanRename
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the Delete context menu option is visible for this wrapper. Default is true.
        /// </summary>
        protected internal virtual bool CanDelete
        {
            get { return true; }
        }

        /// <summary>
        /// Determines whether the Add context menu option is visible for this wrapper. Default is false.
        /// </summary>
        protected internal virtual bool CanAdd
        {
            get { return false; }
        }

        /// <summary>
        /// Determines whether the Encode context menu option is visible for this wrapper. Default is false.
        /// </summary>
        protected internal virtual bool CanEncode
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates if this wrapped object implements the <see cref="ITextureFile"/> interface. Default is false.
        /// </summary>
        protected internal virtual bool IsImageResource
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates if this wrapped object is a model resource. Default is false.
        /// </summary>
        protected internal virtual bool IsModelResource
        {
            get { return false; }
        }

        // Context menu handlers
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

        public void Delete(object sender, EventArgs e)
        {
            Remove();
        }

        public void Rename(object sender, EventArgs e)
        {
            TreeView.LabelEdit = true;
            BeginEdit();
            // Editing is ended in the TreeView AfterLabelEdit event
        }

        public virtual void Export(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDlg = new SaveFileDialog())
            {
                saveFileDlg.FileName = Text;
                saveFileDlg.Filter = "All files (*.*)|*.*";

                if (saveFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                // rebuild data before export
                RebuildWrappedObject();

                (WrappedObject as BinaryFileBase).Save(saveFileDlg.FileName);
            }
        }

        public virtual void Replace(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.FileName = Text;
                openFileDlg.Filter = "All files (*.*)|*.*";

                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                WrappedObject = new GenericBinaryFile(openFileDlg.FileName);

                // re-init
                InitializeWrapper();
            }
        }

        public virtual void Add(object sender, EventArgs e) { }

        public virtual void Encode(object sender, EventArgs e) { }

        // Other methods
        public byte[] GetBytes()
        {
            return (WrappedObject as BinaryFileBase).GetBytes();
        }

        public ResourceProperty GetProperty(string name)
        {
            return Array.Find(_resProperties, item => item.Name == name);
        }

        public T GetPropertyValue<T>(string name)
        {
            return GetProperty(name).GetValue<T>();
        }

        protected internal virtual void RebuildWrappedObject() { }

        protected internal virtual void InitializeCustomPropertyGrid()
        {
            MainForm.Instance.MainPropertyGrid.Item.Clear();
            foreach (ResourceProperty property in ResourceProperties)
            {
                MainForm.Instance.MainPropertyGrid.Item.Add(property.Name, property.Value, false, "", "", true);
            }
            MainForm.Instance.MainPropertyGrid.Refresh();
        }

        protected internal virtual void InitializeWrapper()
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

        private void InitializeContextMenuStrip()
        {
            ContextMenuStrip = new ContextMenuStrip();

            if (CanExport)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Export", null, Export, Keys.Control | Keys.E));
            }

            if (CanReplace)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Replace", null, Replace, Keys.Control | Keys.R));
                if (!CanAdd)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CanAdd)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Add", null, Add, Keys.Control | Keys.A));
                if (CanMove)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CanMove)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUp, Keys.Control | Keys.Up));
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Down", null, MoveDown, Keys.Control | Keys.Down));
            }

            if (CanRename)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Re&name", null, Rename, Keys.Control | Keys.N));
                if (!CanEncode && CanDelete)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CanEncode)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Encode", null, Encode, Keys.Control | Keys.N));
                if (CanDelete)
                    ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (CanDelete)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Delete", null, Delete, Keys.Control | Keys.Delete));
            }
        }
    }
}
