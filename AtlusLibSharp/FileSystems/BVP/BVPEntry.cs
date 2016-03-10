namespace AtlusLibSharp.FileSystems.BVP
{
    using System.IO;
    using Utilities;

    /// <summary>
    /// Class representing an entry in a battle voice package.
    /// </summary>
    public sealed class BVPEntry
    {
        internal const int SIZE = 0xC;

        /// <summary>
        /// Creates a new battle voice package entry using a specified file path and flag.
        /// </summary>
        /// <param name="filePath">Path to the file whose data will be imported.</param>
        /// <param name="flag">Entry flag to set. Defaults to 0.</param>
        public BVPEntry(string filePath, int flag = 0)
        {
            Flag = flag;
            Data = File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// Creates a new battle voice package entry using a specified byte array and flag.
        /// </summary>
        /// <param name="data">Byte array of data.</param>
        /// <param name="flag">Entry flag to set. Defaults to 0.</param>
        public BVPEntry(byte[] data, int flag = 0)
        {
            Flag = flag;
            Data = data;
        }

        /// <summary>
        /// Creates a new battle voice package entry using a specified <see cref="BinaryReader"/>. 
        /// </summary>
        internal BVPEntry(BinaryReader reader)
        {
            Flag = reader.ReadInt32();
            FileDataOffset = reader.ReadInt32();
            int length = reader.ReadInt32();

            if (length > reader.BaseStream.Length)
            {
                Flag = 0;
                FileDataOffset = 0;
                Data = new byte[0];
                return;
            }

            if (length != 0)
            {
                Data = reader.ReadBytesAtOffset(length, FileDataOffset);
            }
            else
            {
                Data = new byte[0];
            }
        }

        /********************/
        /**** Properties ****/
        /********************/

        /// <summary>
        /// Gets or sets the entry flag.
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// Gets or sets the entry data byte array.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets the length of the entry data.
        /// </summary>
        public int DataLength
        {
            get { return Data.Length; }
        }

        /// <summary>
        /// Gets the offset to the data of the entry in the file.
        /// </summary>
        internal int FileDataOffset { get; set; }

        /// <summary>
        /// Write the entry header.
        /// </summary>
        internal void WriteEntryHeader(BinaryWriter writer)
        {
            writer.Write(Flag);
            writer.Write(FileDataOffset);
            writer.Write(DataLength);
        }
    }
}
