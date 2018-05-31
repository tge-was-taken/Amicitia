namespace AmicitiaLibrary.Graphics.TXP
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    using AmicitiaLibrary.Utilities;
    using IO;
    using TMX;

    public class TbFile : BinaryBase
    {
        private const byte HEADER_SIZE = 0x10;
        private const short FLAG = 0x0009;
        internal const string TAG = "TXP0";

        // Private Fields
        private List<TmxFile> mTextures;

        // Constructors
        internal TbFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        public TbFile(IEnumerable<TmxFile> textures)
        {
            mTextures = textures.ToList();
        }

        // Properties
        public int TextureCount
        {
            get { return mTextures.Count; }
        }

        public List<TmxFile> Textures
        {
            get { return mTextures; }
        }

        // Public Static Methods
        public static TbFile LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new TbFile(reader);
        }

        public static TbFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new TbFile(reader);
        }

        // Internal Methods
        internal override void Write(BinaryWriter writer)
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
                mTextures[i].Write(writer);
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
            short userId = reader.ReadInt16();
            int length = reader.ReadInt32();
            string tag = reader.ReadCString(4);
            int unused = reader.ReadInt32();

            if (tag != TAG)
            {
                throw new InvalidDataException();
            }

            int numTextures = reader.ReadInt32();
            int[] texturePointerTable = reader.ReadInt32Array(numTextures);

            mTextures = new List<TmxFile>(numTextures);
            for (int i = 0; i < numTextures; i++)
            {
                reader.BaseStream.Seek(posFileStart + texturePointerTable[i], SeekOrigin.Begin);
                mTextures.Add(TmxFile.Load(reader.BaseStream, true));
            }
        }
    }
}
