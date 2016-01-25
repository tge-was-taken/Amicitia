namespace AtlusLibSharp.SMT3.ChunkResources.Modeling
{
    using System;
    using System.IO;

    using Utilities;

    internal class NDNM
    {
        // Internal Constants
        internal const string NDNM_TAG = "NDNM";

        // Fields
        private IntPtr _ptr;
        private uint _length;
        private MDNodeName[] _names;

        // Constructors
        internal NDNM(uint numNodes, BinaryReader reader)
        {
            _ptr = (IntPtr)reader.BaseStream.Position;
            Read(numNodes, reader);
        }

        // Properties
        public IntPtr Pointer
        {
            get { return _ptr; }
        }

        public MDNodeName[] Names
        {
            get { return _names; }
        }

        // Internal Methods
        internal void Write(BinaryWriter writer)
        {
            // Save start offset and seek past the header
            long startOffset = writer.BaseStream.Position;
            writer.BaseStream.Seek(0x08, SeekOrigin.Current);

            // Write entries
            for (int i = 0; i < _names.Length; i++)
            {
                writer.WriteCStringAligned(_names[i].Name);
                writer.Write(_names[i].ID);
            }

            // Save end offset and calculate the size of the section
            long endOffset = writer.BaseStream.Position;
            _length = (uint)(endOffset - startOffset);

            // Seek back to the start and write the header
            writer.BaseStream.Seek(startOffset, SeekOrigin.Begin);
            writer.WriteCString(NDNM_TAG, 4);
            writer.Write(_length);

            // Seek to the end of the section to align the writer position
            writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);
        }

        // Private Methods
        private void Read(uint numNodes, BinaryReader reader)
        {
            string tag = reader.ReadCString(4);
            if (tag != NDNM_TAG)
            {
                // Shouldn't happen
                throw new InvalidDataException("NDNM Tag mismatch!");
            }

            _length = reader.ReadUInt32();
            _names = new MDNodeName[numNodes];

            for (int i = 0; i < numNodes; i++)
            {
                _names[i].Name = reader.ReadCStringAligned();
                _names[i].ID = reader.ReadInt32();
            }
        }
    }
}
