namespace AtlusLibSharp.FileSystems.PAKToolArchive
{
    using System.IO;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Class representing an entry in a PAKToolFile instance.
    /// </summary>
    public class PakToolArchiveEntry : IArchiveEntry
    {
        internal const int MAX_NAME_LENGTH = 252;
        private string mName;

        /**********************/
        /**** Constructors ****/
        /**********************/

        public PakToolArchiveEntry(string filepath)
        {
            Name = Path.GetFileName(filepath);
            Data = File.ReadAllBytes(filepath);
        }

        public PakToolArchiveEntry(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public PakToolArchiveEntry(string name, MemoryStream stream)
        {
            Name = name;
            Data = stream.ToArray();
        }

        internal PakToolArchiveEntry(BinaryReader reader)
        {
            InternalRead(reader);
        }

        /********************/
        /**** Properties ****/
        /********************/

        /// <summary>
        /// Gets or sets the archive entry name.
        /// </summary>
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                if (value.Length > MAX_NAME_LENGTH)
                {
                    mName = mName.Remove(MAX_NAME_LENGTH - 1);
                }
                else
                {
                    mName = value;
                }
            }
        }

        IArchiveEntry IArchiveEntry.Create(byte[] data, string name)
        {
            return new PakToolArchiveEntry(name, data);
        }

        /// <summary>
        /// Gets the length of the archive entry data.
        /// </summary>
        public int DataLength
        {
            get { return Data.Length; }
        }

        /// <summary>
        /// Gets the archive entry data.
        /// </summary>
        public byte[] Data { get; set; }

        /*****************/
        /**** Methods ****/
        /*****************/

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.WriteCString(mName, MAX_NAME_LENGTH);
            writer.Write(DataLength);
            writer.Write(Data);
            writer.AlignPosition(64);
        }

        private void InternalRead(BinaryReader reader)
        {
            mName = reader.ReadCString(MAX_NAME_LENGTH);
            int dataLength = reader.ReadInt32();

            if (dataLength > reader.BaseStream.Length || dataLength < 0)
            {
                throw new InvalidDataException("dataLength has an invalid value.");
            }

            Data = reader.ReadBytes(dataLength);
            reader.AlignPosition(64);
        }
    }
}
