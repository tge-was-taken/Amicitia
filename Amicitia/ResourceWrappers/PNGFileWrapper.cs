using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Amicitia.ResourceWrappers
{
    internal class PNGFileWrapper : ResourceWrapper
    {
        protected internal static readonly SupportedFileType[] FileFilterTypes = new SupportedFileType[]
        {
            SupportedFileType.PNGFile
        };

        public PNGFileWrapper(string text, Bitmap bitmap) 
            : base(text, bitmap)
        {
        }

        public SupportedFileType FileType
        {
            get { return SupportedFileType.PNGFile; }
        }

        [Category("Texture info")]
        public int Width
        {
            get { return WrappedObject.Width; }
        }

        [Category("Texture info")]
        public int Height
        {
            get { return WrappedObject.Height; }
        }

        protected internal new Bitmap WrappedObject
        {
            get { return (Bitmap)base.WrappedObject; }
            set { base.WrappedObject = value; }
        }

        protected internal override bool IsImageResource
        {
            get { return true; }
        }

        // TODO: Implement texture encoder form
        /*
        protected internal override bool CanEncode
        {
            get { return true; }
        }
        */

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
                    case SupportedFileType.PNGFile:
                        WrappedObject.Save(saveFileDlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
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
                    case SupportedFileType.PNGFile:
                        WrappedObject = new Bitmap(
                            openFileDlg.FileName);
                        break;

                }

                // re-init wrapper
                InitializeWrapper();
            }
        }
    }
}
