namespace Amicitia.ResourceWrappers
{
    using AtlusLibSharp;
    using System;
    using System.Windows.Forms;

    internal enum GetWrapperOptions
    {
        None,
        ForceRebuild
    }

    internal partial class ResourceWrapper : TreeNode
    {
        private object _wrappedObject;
        private Type _wrappedType;
        private bool _isInitialized;
        private ResourceProperty[] _resProperties;

        public ResourceWrapper(string text, object wrappedObject, params ResourceProperty[] properties)
            : base(text)
        {
            _wrappedObject = wrappedObject;
            _wrappedType = wrappedObject.GetType();
            _resProperties = properties;
            InitializeContextMenuStrip();
            InitializeWrapper();
        }

        // Properties
        protected internal Type WrappedType
        {
            get { return _wrappedType; }
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
        protected internal virtual bool IsExportable
        {
            get { return true; }
        }

        protected internal virtual bool IsReplaceable
        {
            get { return true; }
        }

        protected internal virtual bool IsMoveable
        {
            get { return true; }
        }

        protected internal virtual bool IsRenameable
        {
            get { return true; }
        }

        protected internal virtual bool IsDeleteable
        {
            get { return true; }
        }

        protected internal virtual bool IsImageResource
        {
            get { return false; }
        }

        // Event handlers
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

                GetWrappedObject<GenericBinaryFile>().Save(saveFileDlg.FileName);
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

                ReplaceWrappedObjectAndInitialize(new GenericBinaryFile(openFileDlg.FileName));
            }
        }

        // Public Methods
        public byte[] GetBytes()
        {
            RebuildWrappedObject();
            return GetWrappedObject<BinaryFileBase>().GetBytes();
        }

        // Property Methods
        public ResourceProperty GetProperty(string name)
        {
            return Array.Find(_resProperties, item => item.Name == name);
        }

        public T GetPropertyValue<T>(string name)
        {
            return GetProperty(name).GetValue<T>();
        }

        // Protected Methods
        protected internal T GetWrappedObject<T>(GetWrapperOptions options = GetWrapperOptions.None)
        {
            if (options == GetWrapperOptions.ForceRebuild)
            {
                RebuildWrappedObject();
            }

            return (T)_wrappedObject;
        }

        protected internal virtual void InitializeCustomPropertyGrid()
        {
            MainForm.Instance.MainPropertyGrid.Item.Clear();
            foreach (ResourceProperty property in ResourceProperties)
            {
                MainForm.Instance.MainPropertyGrid.Item.Add(property.Name, property.Value, false, "", "", true);
            }
            MainForm.Instance.MainPropertyGrid.Refresh();
        }

        protected virtual void InitializeWrapper()
        {
            if (IsInitialized)
            {

            }
            else
            {
                IsInitialized = true;
            }
        }

        protected virtual void RebuildWrappedObject()
        {

        }

        protected void ReplaceWrappedObjectAndInitialize<T>(T newObject)
        {
            _wrappedObject = null;
            _wrappedObject = newObject;
            InitializeWrapper();
        }

        // Private Methods
        private void InitializeContextMenuStrip()
        {
            ContextMenuStrip = new ContextMenuStrip();

            if (IsExportable)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Export", null, Export, Keys.Control | Keys.E));
            }

            if (IsReplaceable)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Replace", null, Replace, Keys.Control | Keys.R));
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (IsMoveable)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Up", null, MoveUp, Keys.Control | Keys.Up));
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Move &Down", null, MoveDown, Keys.Control | Keys.Down));
            }

            if (IsRenameable)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("Re&name", null, Rename, Keys.Control | Keys.N));
                ContextMenuStrip.Items.Add(new ToolStripSeparator());
            }

            if (IsDeleteable)
            {
                ContextMenuStrip.Items.Add(new ToolStripMenuItem("&Delete", null, Delete, Keys.Control | Keys.Delete));
            }
        }
    }
}
