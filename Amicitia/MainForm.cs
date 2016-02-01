using System;
using System.Windows.Forms;
using System.IO;
using Amicitia.ResourceWrappers;
using Amicitia.Utilities;
using AtlusLibSharp.SMT3.ChunkResources.Graphics;
using AtlusLibSharp.Utilities;

namespace Amicitia
{
    public partial class MainForm : Form
    {
        private static MainForm _instance;

        internal static MainForm Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                if (_instance != null)
                {
                    throw new Exception("Instance already exists!");
                }

                _instance = value;
            }
        }

        internal TreeView MainTreeView
        {
            get { return mainTreeView; }
        }

        internal PropertyGridEx.PropertyGridEx MainPropertyGrid
        {
            get { return mainPropertyGrid; }
        }

        internal PictureBox MainPictureBox
        {
            get { return mainPictureBox; }
        }

        public MainForm()
        {
            InitializeComponent();
            InitializeMainForm();
        }

        // Events
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (filePaths.Length > 0)
            {
                HandleFileOpenFromPath(filePaths[0]);
            }
        }

        private void MainTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            mainTreeView.LabelEdit = false;
        }

        private void MainTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                HandleTreeViewCtrlShortcuts(e.KeyData);
            }
        }

        private void MainTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ResourceWrapper res = (ResourceWrapper)mainTreeView.SelectedNode;
            mainPropertyGrid.SelectedObject = res;

            // Check if the resource is a texture
            if (res.IsImageResource == true)
            {
                // Unhide the picture box if we have a picture selected
                mainPictureBox.Visible = true;

                // TODO: Implement generic texture interface
                mainPictureBox.Image = res.GetWrappedObject<TMXFile>().GetBitmap();
            }
            else
            {
                // If we don't have a texture selected then keep it invisible
                mainPictureBox.Visible = false;
            }

            res.InitializeCustomPropertyGrid();
        }

        private void MainTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Setting this will make all mouse clicks select a node, as opposed to only left clicks
            mainTreeView.SelectedNode = e.Node;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDlg = new OpenFileDialog())
            {
                openFileDlg.Filter = SupportedFileHandler.FileFilter;

                // Exit out if user didn't select a file
                if (openFileDlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                HandleFileOpenFromPath(openFileDlg.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mainTreeView.Nodes.Count > 0)
            {
                ((ResourceWrapper)mainTreeView.TopNode).Export(sender, e);
            }
        }

        // Internal methods
        internal void UpdateReferences()
        {
            MainTreeView_AfterSelect(this, new TreeViewEventArgs(mainTreeView.SelectedNode));
        }

        // Initializer
        private void InitializeMainForm()
        {
            Instance = this;
            this.DragDrop += MainForm_DragDrop;
            this.DragEnter += MainForm_DragEnter;
            mainTreeView.AfterSelect += MainTreeView_AfterSelect;
            mainTreeView.KeyDown += MainTreeView_KeyDown;
            mainTreeView.AfterLabelEdit += MainTreeView_AfterLabelEdit;
            mainTreeView.NodeMouseClick += MainTreeView_NodeMouseClick;
            mainPictureBox.Visible = false;
            mainPropertyGrid.ShowCustomProperties = true;
            mainPropertyGrid.PropertySort = PropertySort.NoSort;
        }

        // Handlers
        private void HandleTreeViewCtrlShortcuts(Keys keys)
        {
            ResourceWrapper res = (ResourceWrapper)mainTreeView.SelectedNode;

            // Move up
            if (keys.HasFlagFast(Keys.Up))
            {
                if (res.IsMoveable)
                {
                    res.MoveUp(this, EventArgs.Empty);
                }
            }

            // Move down
            else if (keys.HasFlagFast(Keys.Down))
            {
                if (res.IsMoveable)
                {
                    res.MoveDown(this, EventArgs.Empty);
                }
            }

            // Delete
            else if (keys.HasFlagFast(Keys.Delete))
            {
                res.Delete(this, EventArgs.Empty);
            }

            // Replace
            else if (keys.HasFlagFast(Keys.R))
            {
                if (res.IsReplaceable)
                {
                    res.Replace(this, EventArgs.Empty);
                }
            }

            // Rename
            else if (keys.HasFlagFast(Keys.E))
            {
                if (res.IsRenameable)
                {
                    res.Export(this, EventArgs.Empty);
                }
            }
        }

        private void HandleFileOpenFromPath(string filePath)
        {
            // Get the supported file index so we can check if it's /actually/ supported as you can override the filter easily by copy pasting
            int supportedFileIndex = SupportedFileHandler.GetSupportedFileIndex(filePath);

            if (supportedFileIndex == -1)
            {
                return;
            }

            // Hide the picture box so a possibly last selected image doesn't stay visible
            if (mainPictureBox.Visible == true)
            {
                mainPictureBox.Visible = false;
                mainPictureBox.Image.Dispose();
            }

            // Clear nodes as we don't want multiple hierarchies
            if (mainTreeView.Nodes.Count > 0)
            {
                mainTreeView.Nodes.Clear();
            }

            TreeNode treeNode = null;

#if !DEBUG
            try
            {
#endif
                // Get the resource from the factory and it to the tree view
                treeNode = ResourceFactory.GetResource(
                    Path.GetFileName(filePath),
                    File.OpenRead(filePath), supportedFileIndex);
#if !DEBUG
            }
            catch (InvalidDataException exception)
            {
                MessageBox.Show("Data was not in expected format, can't open file.\n Stacktrace:\n" + exception.StackTrace, "Open file error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
#endif

            mainTreeView.BeginUpdate();

            mainTreeView.Nodes.Add(treeNode);
            mainTreeView.SelectedNode = mainTreeView.TopNode;

            mainTreeView.EndUpdate();
        }
    }
}
