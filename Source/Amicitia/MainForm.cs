using System;
using System.Windows.Forms;
using System.IO;
using Amicitia.ResourceWrappers;
using AtlusLibSharp.Utilities;
using AtlusLibSharp.Graphics;
using OpenTK;
using System.Runtime.InteropServices;
using Amicitia.ModelViewer;
using AtlusLibSharp.Graphics.RenderWare;
using System.Drawing;

namespace Amicitia
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();


        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
    }

    public partial class MainForm : Form
    {
        //private static Rectangle _lastMainTreeViewSize;
        private static MainForm _instance;
        private ModelViewer.ModelViewer viewer;

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

        internal PropertyGrid MainPropertyGrid
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
#if DEBUG
            NativeMethods.AllocConsole();
#endif
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
            // hide the picture box
            mainPictureBox.Visible = false;
            viewer.DeleteScene();
            glControl1.Visible = false;
            ResourceWrapper res = mainTreeView.SelectedNode as ResourceWrapper;

            mainPropertyGrid.SelectedObject = res;

            if (res == null)
                return;

            if(res.IsModelResource == true)
            {
                try
                {
                    viewer.LoadScene((res as ResourceWrapper).WrappedObject as RMDScene);
                    glControl1.Focus();
                    glControl1.Invalidate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
                glControl1.Visible = true;
            }
            // Check if the resource is a texture
            if (res.IsImageResource == true)
            {
                // Unhide the picture box if we have a picture selected
                mainPictureBox.Visible = true;
                mainPictureBox.Image = ((ITextureFile)res.WrappedObject).GetBitmap();
            }

            //res.InitializeCustomPropertyGrid();
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
                openFileDlg.Filter = SupportedFileManager.FileFilter;

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
            // this
            Instance = this;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            //MouseMove += MainForm_MouseMove;
            //_lastMainTreeViewSize = mainTreeView.Bounds;

            // mainTreeView
            mainTreeView.AfterSelect += MainTreeView_AfterSelect;
            mainTreeView.KeyDown += MainTreeView_KeyDown;
            mainTreeView.AfterLabelEdit += MainTreeView_AfterLabelEdit;
            mainTreeView.NodeMouseClick += MainTreeView_NodeMouseClick;
            //mainTreeView.SizeChanged += MainTreeView_SizeChanged;
            
            // mainPictureBox
            mainPictureBox.Visible = false;

            // mainPropertyGrid
            mainPropertyGrid.PropertySort = PropertySort.NoSort;
        }

        /*
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X == mainPictureBox.Location.X + mainPictureBox.Size.Width)
            {
                Cursor.Current = Cursors.SizeWE;
            }
        }

        private void MainTreeView_SizeChanged(object sender, EventArgs e)
        {
            float widthDiff = mainTreeView.Bounds.Width - _lastMainTreeViewSize.Width;

            MoveDockedObject(mainPictureBox, (int)widthDiff * 2, 0);
            MoveDockedObject(mainPropertyGrid, (int)widthDiff * 2, 0);

            if (mainPictureBox.Bounds.IntersectsWith(mainTreeView.Bounds) || mainPropertyGrid.Bounds.IntersectsWith(mainTreeView.Bounds))
            {
                
                MoveDockedObject(mainPictureBox, -2, 0);
                MoveDockedObject(mainPropertyGrid, -2, 0);
                
            }

            _lastMainTreeViewSize = mainTreeView.Bounds;
        }

        private void MoveDockedObject(Control control, int width, int height)
        {
            Rectangle old = control.Bounds;
            old.X += width;
            old.Y += height;
            control.Bounds = old;
        }
        */

        // Handlers
        private void HandleTreeViewCtrlShortcuts(Keys keys)
        {
            ResourceWrapper res = (ResourceWrapper)mainTreeView.SelectedNode;

            // Move up
            if (keys.HasFlagUnchecked(Keys.Up))
            {
                if (res.CanMove)
                {
                    res.MoveUp(this, EventArgs.Empty);
                }
            }

            // Move down
            else if (keys.HasFlagUnchecked(Keys.Down))
            {
                if (res.CanMove)
                {
                    res.MoveDown(this, EventArgs.Empty);
                }
            }

            // Delete
            else if (keys.HasFlagUnchecked(Keys.Delete))
            {
                if (res.CanDelete)
                {
                    res.Delete(this, EventArgs.Empty);
                }
            }

            // Replace
            else if (keys.HasFlagUnchecked(Keys.R))
            {
                if (res.CanReplace)
                {
                    res.Replace(this, EventArgs.Empty);
                }
            }

            // Rename
            else if (keys.HasFlagUnchecked(Keys.E))
            {
                if (res.CanRename)
                {
                    res.Export(this, EventArgs.Empty);
                }
            }
        }

        private void HandleFileOpenFromPath(string filePath)
        {
            if (viewer.IsSceneReady == true) viewer.DeleteScene();
            // Get the supported file index so we can check if it's /actually/ supported as you can override the filter easily by copy pasting
            int supportedFileIndex = SupportedFileManager.GetSupportedFileIndex(filePath);

            if (supportedFileIndex == -1)
            {
                return;
            }

            // Hide the picture box so a possibly last selected image doesn't stay visible
            if (mainPictureBox.Visible == true)
            {
                mainPictureBox.Visible = false;
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

        private void mainPictureBox_Click(object sender, EventArgs e)
        {

        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            glControl1.Visible = false;
            viewer = new ModelViewer.ModelViewer(glControl1);
        }

        private void mainTreeView_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }

        private void mainMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            viewer.DisposeViewer();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form options = new Form();
            Label colorlab = new Label();
            Label more = new Label();
            Button picker = new Button();
            options.Text = "Options";
            more.Text = "Stay tuned...";
            more.Location = new Point(16, 80);
            more.ForeColor = Color.Gray;
            more.Font = new Font(more.Font, FontStyle.Italic);
            picker.Text = "...";
            picker.Location = new System.Drawing.Point(140, 16);
            picker.Width /= 3;
            picker.Click += (object s, EventArgs ev) =>
            {
                ColorDialog d = new ColorDialog();
                d.Color = viewer.BGColor;
                d.ShowDialog(options);
                viewer.BGColor = d.Color;
            };
            colorlab.Text = "Model viewer bg color";
            colorlab.Location = new System.Drawing.Point(16, 20);
            colorlab.Width = 200;
            options.Size = new System.Drawing.Size(512, 256);
            options.Controls.Add(picker);
            options.Controls.Add(colorlab);
            options.Controls.Add(more);
            options.ShowDialog(this);
        }
    }
}
