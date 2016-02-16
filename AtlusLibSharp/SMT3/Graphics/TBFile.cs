namespace AtlusLibSharp.SMT3.Graphics
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    using Common.Utilities;
    using Common;

    public class TBFile : BinaryFileBase
    {
        // Internal Constants
        internal const byte HEADER_SIZE = 0x10;
        internal const short FLAG = 0x0009;
        internal const string TAG = "TXP0";

        // Private Fields
        private TMXFile[] _textures;

        // Constructors
        internal TBFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        public TBFile(IList<TMXFile> textures)
        {
            _textures = textures.ToArray();
        }

        // Properties
        public int TextureCount
        {
            get { return _textures.Length; }
        }

        public TMXFile[] Textures
        {
            get { return _textures; }
        }

        // Public Static Methods
        public static TBFile LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new TBFile(reader);
        }

        public static TBFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new TBFile(reader);
        }

        // Internal Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            int posFileStart = (int)writer.BaseStream.Position;

            // Seek past chunk header
            writer.BaseStream.Seek(HEADER_SIZE + 4, SeekOrigin.Current);

            // Seek past texture pack header
            writer.BaseStream.Seek(TextureCount * sizeof(int), SeekOrigin.Current);

            int[] texturePointerTable = new int[TextureCount];

            // Write texture data
            for (int i = 0; i < TextureCount; i++)
            {
                writer.AlignPosition(64);
                texturePointerTable[i] = (int)(writer.BaseStream.Position - posFileStart);
                _textures[i].InternalWrite(writer);
            }

            // Calculate length
            long posFileEnd = writer.BaseStream.Position;
            int length = (int)(posFileEnd - posFileStart);

            // Write header
            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);
            writer.Write(FLAG);
            writer.Write((short)0); // userID
            writer.Write(length);
            writer.WriteCString(TAG, 4);
            writer.Write(0);
            writer.Write(TextureCount);

            // Write pointer table
            writer.Write(texturePointerTable);
            
            // Seek to end
            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
        }

        // Private Methods
        private void InternalRead(BinaryReader reader)
        {
            int posFileStart = (int)reader.BaseStream.Position;
            short flag = reader.ReadInt16();
            short userID = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            int unused = reader.ReadInt32();

            if (tag != TAG)
            {
                throw new InvalidDataException();
            }

            int numTextures = reader.ReadInt32();
            int[] texturePointerTable = reader.ReadInt32Array(numTextures);

            _textures = new TMXFile[numTextures];
            for (int i = 0; i < _textures.Length; i++)
            {
                reader.BaseStream.Seek(posFileStart + texturePointerTable[i], SeekOrigin.Begin);
                _textures[i] = TMXFile.LoadFrom(reader.BaseStream, true);
            }
        }
    }
}
