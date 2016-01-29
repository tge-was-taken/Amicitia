namespace AtlusLibSharp.SMT3.ChunkResources.Graphics
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    using Utilities;

    public class TXPChunk : Chunk
    {
        // Internal Constants
        internal const int    FLAG = 0x0009;
        internal const string TAG  = "TXP0";

        // Private Fields
        private int _numTextures;
        private int[] _texturePointerTable;
        private TMXChunk[] _textures;

        // Constructors
        internal TXPChunk(ushort id, int length, BinaryReader reader)
            : base(FLAG, id, length, TAG)
        {
            Read(reader);
        }

        public TXPChunk(IList<TMXChunk> textures)
            : base(FLAG, 0, 0, TAG)
        {
            _numTextures = textures.Count;
            _textures = textures.ToArray();
        }

        // Properties
        public int TextureCount
        {
            get { return _numTextures; }
        }

        public TMXChunk[] Textures
        {
            get { return _textures; }
        }

        // Internal Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            int fp = (int)writer.BaseStream.Position;

            // Seek past chunk header
            writer.BaseStream.Seek(HEADER_SIZE+8, SeekOrigin.Current);

            // Seek past texture pack header
            writer.BaseStream.Seek(_numTextures * sizeof(int), SeekOrigin.Current);

            // Write texture data
            for (int i = 0; i < _numTextures; i++)
            {
                writer.AlignPosition(64);
                _texturePointerTable[i] = (int)(writer.BaseStream.Position - fp);
                _textures[i].InternalWrite(writer);
            }

            // Calculate length
            long endOffset = writer.BaseStream.Position;
            Length = (int)(endOffset - fp);

            // Write header
            writer.BaseStream.Seek(fp, SeekOrigin.Begin);
            writer.Write(Flags);
            writer.Write(UserID);
            writer.Write(Length);
            writer.WriteCString(Tag, 4);
            writer.Write(0);
            writer.Write(_numTextures);

            // Write pointer table
            writer.Write(_texturePointerTable);
            
            // Seek to end, write padding
            writer.BaseStream.Seek(endOffset, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        // Private Methods
        private void Read(BinaryReader reader)
        {
            int fp = (int)reader.BaseStream.Position - HEADER_SIZE;
            reader.AlignPosition(16);

            _numTextures = reader.ReadInt32();
            _texturePointerTable = reader.ReadInt32Array(_numTextures);

            _textures = new TMXChunk[_numTextures];
            for (int i = 0; i < _numTextures; i++)
            {
                reader.BaseStream.Seek(fp + _texturePointerTable[i], SeekOrigin.Begin);
                _textures[i] = ChunkFactory.Get<TMXChunk>(reader);
            }
        }
    }
}
