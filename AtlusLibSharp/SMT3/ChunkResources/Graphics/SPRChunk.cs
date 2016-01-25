namespace AtlusLibSharp.SMT3.ChunkResources.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Utilities;

    public class SPRChunk : Chunk
    {
        // Internal Constants
        internal const ushort SPR0_FLAGS = 0x0001;
        internal const string SPR0_TAG = "SPR0";
        internal const int    SPR0_HEADER_SIZE = 0x20;
        internal const int SPR0_TABLE_ENTRY_SIZE = 0x08;

        // Private Fields
        private int _headerSize;
        private int _fileSizeMin4;
        private ushort _numTextures;
        private ushort _numKeyFrames;
        private int _texturePointerTableOffset;
        private int _keyFramePointerTableOffset;
        private int[] _texturePointerTable;
        private int[] _keyFramePointerTable;
        private TMXChunk[] _textures;
        private SPRKeyFrame[] _keyFrames;

        // Constructors
        internal SPRChunk(ushort id, ref int length, BinaryReader reader)
            : base(SPR0_FLAGS, id, length, SPR0_TAG)
        {
            Read(reader);
            FixLength(ref length);
        }

        public SPRChunk(IList<SPRKeyFrame> keyFrames, IList<TMXChunk> textures)
            : base(SPR0_FLAGS, 0, 0, SPR0_TAG)
        {
            _headerSize = SPR0_HEADER_SIZE;
            _numTextures = (ushort)textures.Count;
            _numKeyFrames = (ushort)keyFrames.Count;
            _textures = new TMXChunk[_numTextures];
            _keyFrames = new SPRKeyFrame[_numKeyFrames];
            textures.CopyTo(_textures, 0);
            keyFrames.CopyTo(_keyFrames, 0);
        }

        // Properties
        public int TextureCount
        {
            get { return _numTextures; }
        }

        public int KeyFrameCount
        {
            get { return _numKeyFrames; }
        }

        public SPRKeyFrame[] KeyFrames
        {
            get
            {
                return _keyFrames;
            }
            private set
            {
                _keyFrames = value;
                _numKeyFrames = (ushort)_keyFrames.Length;
            }
        }

        public TMXChunk[] Textures
        {
            get
            {
                return _textures;
            }
            private set
            {
                _textures = value;
                _numTextures = (ushort)_textures.Length;
            }
        }

        // Public Methods
        public static SPRChunk LoadFrom(string path)
        {
            return ChunkFactory.Get<SPRChunk>(path);
        }

        public static SPRChunk LoadFrom(Stream stream)
        {
            return ChunkFactory.Get<SPRChunk>(stream);
        }

        // Internal Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            // Save the start position to calculate the filesize and 
            // to write out the header after we know where all the structure offsets are
            int filePtr = (int)writer.BaseStream.Position;
            writer.BaseStream.Seek(SPR0_HEADER_SIZE, SeekOrigin.Current);

            // Set the pointer table offsets and seek past the entries
            // as the entries will be written later
            _texturePointerTableOffset = (int)(writer.BaseStream.Position - filePtr);
            writer.BaseStream.Seek(SPR0_TABLE_ENTRY_SIZE * _numTextures, SeekOrigin.Current);

            _keyFramePointerTableOffset = (int)(writer.BaseStream.Position - filePtr);
            writer.BaseStream.Seek(SPR0_TABLE_ENTRY_SIZE * _numKeyFrames, SeekOrigin.Current);

            // Write out the keyframe data and fill up the pointer table
            _keyFramePointerTable = new int[_numKeyFrames];
            for (int i = 0; i < _numKeyFrames; i++)
            {
                writer.AlignPosition(16);
                _keyFramePointerTable[i] = (int)(writer.BaseStream.Position - filePtr);
                _keyFrames[i].Write(writer);
            }

            // Write out the texture data and fill up the pointer table
            _texturePointerTable = new int[_numTextures];
            for (int i = 0; i < _numTextures; i++)
            {
                writer.AlignPosition(16);
                _texturePointerTable[i] = (int)(writer.BaseStream.Position - filePtr);
                _textures[i].InternalWrite(writer);
            }

            // Write out padding at the end of the file
            writer.AlignPosition(64);

            // Save the end position to calculate the file size
            long endOfFilePos = writer.BaseStream.Position;
            _fileSizeMin4 = (int)(endOfFilePos - filePtr) - 4;
            Length = _fileSizeMin4 + 4;

            // Seek back to the tables and write out the offsets
            writer.BaseStream.Seek(filePtr + _texturePointerTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < _numTextures; i++)
            {
                writer.Write(0);
                writer.Write(_texturePointerTable[i]);
            }

            writer.BaseStream.Seek(filePtr + _keyFramePointerTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < _numKeyFrames; i++)
            {
                writer.Write(0);
                writer.Write(_keyFramePointerTable[i]);
            }

            // Seek back to the file and write out the header with
            // the offsets to the structures in the file
            writer.BaseStream.Seek(filePtr, SeekOrigin.Begin);
            writer.Write(Flags);
            writer.Write(UserID);
            writer.Write(0);
            writer.WriteCString(Tag, 4);
            writer.Write(_headerSize);
            writer.Write(_fileSizeMin4);
            writer.Write(_numTextures);
            writer.Write(_numKeyFrames);
            writer.Write(_texturePointerTableOffset);
            writer.Write(_keyFramePointerTableOffset);

            // Set the file pointer back to the end of the file as expected
            writer.BaseStream.Seek(endOfFilePos, SeekOrigin.Begin);
        }

        // Private Methods
        private void Read(BinaryReader reader)
        {
            int filePtr = (int)reader.BaseStream.Position - CHUNK_HEADER_SIZE;
            _headerSize = reader.ReadInt32();
            _fileSizeMin4 = reader.ReadInt32();
            _numTextures = reader.ReadUInt16();
            _numKeyFrames = reader.ReadUInt16();
            _texturePointerTableOffset = reader.ReadInt32();
            _keyFramePointerTableOffset = reader.ReadInt32();

            reader.BaseStream.Seek(filePtr + _texturePointerTableOffset, SeekOrigin.Begin);
            _texturePointerTable = new int[_numTextures];
            for (int i = 0; i < _numTextures; i++)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                _texturePointerTable[i] = reader.ReadInt32();
            }

            reader.BaseStream.Seek(filePtr + _keyFramePointerTableOffset, SeekOrigin.Begin);
            _keyFramePointerTable = new int[_numKeyFrames];
            for (int i = 0; i < _numKeyFrames; i++)
            {
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                _keyFramePointerTable[i] = reader.ReadInt32();
            }

            _textures = new TMXChunk[_numTextures];
            for (int i = 0; i < _numTextures; i++)
            {
                reader.BaseStream.Seek(filePtr + _texturePointerTable[i], SeekOrigin.Begin);
                _textures[i] = ChunkFactory.Get<TMXChunk>(reader);
            }

            _keyFrames = new SPRKeyFrame[_numKeyFrames];
            for (int i = 0; i < _numKeyFrames; i++)
            {
                reader.BaseStream.Seek(filePtr + _keyFramePointerTable[i], SeekOrigin.Begin);
                _keyFrames[i] = new SPRKeyFrame(reader);
            }
        }

        private void FixLength(ref int length)
        {
            length = _fileSizeMin4 + 4;
            Length = length;
        }
    }
}
