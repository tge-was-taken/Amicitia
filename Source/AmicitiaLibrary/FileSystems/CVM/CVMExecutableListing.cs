using System.IO;

namespace AmicitiaLibrary.FileSystems.CVM
{
    public class CvmExecutableListing
    {
        private CvmDirectoryListing mRootDirectoryListing;

        public CvmDirectoryListing RootDirectoryListing
        {
            get { return mRootDirectoryListing; }
        }

        public CvmExecutableListing(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) 
                mRootDirectoryListing = new CvmDirectoryListing(reader, null);
        }

        public CvmExecutableListing(Stream stream, bool leaveOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen ) )
                mRootDirectoryListing = new CvmDirectoryListing(reader, null);
        }

        public void Update(CvmFile cvm)
        {
            /*
            if (cvm.RootDirectory.SubEntries.Count != _rootDirectoryListing.SubEntries.Length)
            {
                throw new System.Exception("Error: Number of files in CVM root directory does not match the number of files in the executable listing!");
            }
            */

            mRootDirectoryListing.Update(cvm.RootDirectory);
        }

        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(path), System.Text.Encoding.Default, true))
                mRootDirectoryListing.InternalWrite(writer);
        }

        public void Save(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
                mRootDirectoryListing.InternalWrite(writer);
        }
    }
} 
