using System.IO;

namespace AtlusLibSharp.Persona3.FileSystem.CRIWare
{
    public class CVMExecutableListing
    {
        private CVMDirectoryListing _rootDirectoryListing;

        public CVMDirectoryListing RootDirectoryListing
        {
            get { return _rootDirectoryListing; }
        }

        public CVMExecutableListing(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) 
                _rootDirectoryListing = new CVMDirectoryListing(reader, null);
        }

        public CVMExecutableListing(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
                _rootDirectoryListing = new CVMDirectoryListing(reader, null);
        }

        public void Update(CVMFile cvm)
        {
            if (cvm.RootDirectory.SubEntries.Count != _rootDirectoryListing.SubEntries.Length)
            {
                throw new System.Exception("Error: Number of files in CVM root directory does not match the number of files in the executable listing!");
            }

            _rootDirectoryListing.Update(cvm.RootDirectory);
        }

        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(path), System.Text.Encoding.Default, true))
                _rootDirectoryListing.InternalWrite(writer);
        }

        public void Save(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
                _rootDirectoryListing.InternalWrite(writer);
        }
    }
} 
