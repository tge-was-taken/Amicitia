namespace AtlusLibSharp.Graphics.SPR
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using AtlusLibSharp.Utilities;
    using IO;
    using TGA;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct SPR4Header
    {
        public const ushort FLAGS = 0x0001;
        public const string TAG = "SPR4";
        public const int SIZE = 0x20;

        [FieldOffset(0)]
        public ushort flags;

        [FieldOffset(2)]
        public ushort userId;

        [FieldOffset(4)]
        public int reserved1;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string tag;

        [FieldOffset(12)]
        public int headerSize;

        [FieldOffset(16)]
        public int deprecatedSize;

        [FieldOffset(20)]
        public ushort numTextures;

        [FieldOffset(22)]
        public ushort numKeyFrames;

        [FieldOffset(24)]
        public int texturePointerTableOffset;

        [FieldOffset(28)]
        public int keyFramePointerTableOffset;

        public SPR4Header(int textureCount, int keyFrameCount)
        {
            flags = FLAGS;
            userId = 0;
            reserved1 = 0;
            tag = TAG;
            headerSize = SIZE;
            deprecatedSize = 0;
            numTextures = (ushort)textureCount;
            numKeyFrames = (ushort)keyFrameCount;
            texturePointerTableOffset = 0;
            keyFramePointerTableOffset = 0;
        }
    }

    public class SPR4File : BinaryFileBase
    {
        private List<TGAFile> _textures;
        private List<SPRKeyFrame> _keyFrames;

        internal SPR4File(BinaryReader reader)
        {
            InternalRead(reader);
        }

        public SPR4File()
        {
            _textures = new List<TGAFile>();
            _keyFrames = new List<SPRKeyFrame>();
        }

        public SPR4File(List<TGAFile> textures, List<SPRKeyFrame> keyframes)
        {
            _textures = textures;
            _keyFrames = keyframes;
        }

        public int TextureCount
        {
            get { return _textures.Count; }
        }

        public int KeyFrameCount
        {
            get { return _keyFrames.Count; }
        }

        public List<TGAFile> Textures
        {
            get { return _textures; }
        }

        public List<SPRKeyFrame> KeyFrames
        {
            get { return _keyFrames; }
        }

        public static SPR4File LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default, true))
                return new SPR4File(reader);
        }

        public static SPR4File LoadFrom(Stream stream, bool leaveStreamOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new SPR4File(reader);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            // Save the start position to calculate the filesize and 
            // to write out the header after we know where all the structure offsets are
            Stream stream = writer.BaseStream;
            int posFileStart = (int)stream.Position;
            stream.Seek(SPR4Header.SIZE, SeekOrigin.Current);

            // Create initial header and tables
            SPR4Header header = new SPR4Header(TextureCount, KeyFrameCount);
            TypePointerTable[] keyFramePointerTable = new TypePointerTable[header.numKeyFrames];
            TypePointerTable[] texturePointerTable = new TypePointerTable[header.numTextures];

            // Set the pointer table offsets and seek past the entries
            // as the entries will be written later
            header.texturePointerTableOffset = (int)(stream.Position - posFileStart);
            stream.Seek(TypePointerTable.SIZE * header.numTextures, SeekOrigin.Current);

            header.keyFramePointerTableOffset = (int)(stream.Position - posFileStart);
            stream.Seek(TypePointerTable.SIZE * header.numKeyFrames, SeekOrigin.Current);

            // Write out the keyframe data and fill up the pointer table
            for (int i = 0; i < header.numKeyFrames; i++)
            {
                writer.AlignPosition(16);
                keyFramePointerTable[i].offset = (int)(stream.Position - posFileStart);
                _keyFrames[i].InternalWrite(writer);
            }

            writer.Seek(16, SeekOrigin.Current);
            writer.AlignPosition(16);

            // Write out the texture data and fill up the pointer table
            for (int i = 0; i < header.numTextures; i++)
            {
                texturePointerTable[i].offset = (int)(stream.Position - posFileStart);
                _textures[i].InternalWrite(writer);
                writer.Seek(16, SeekOrigin.Current);
            }

            // Save the end position
            long posFileEnd = stream.Position;

            // Seek back to the tables and write out the tables
            stream.Seek(posFileStart + header.texturePointerTableOffset, SeekOrigin.Begin);
            stream.WriteStructures(texturePointerTable, header.numTextures);

            stream.Seek(posFileStart + header.keyFramePointerTableOffset, SeekOrigin.Begin);
            stream.WriteStructures(keyFramePointerTable, header.numKeyFrames);

            // Seek back to the file and write out the header with
            // the offsets to the structures in the file
            writer.BaseStream.Seek(posFileStart, SeekOrigin.Begin);
            writer.BaseStream.WriteStructure(header);

            // Set the file pointer back to the end of the file as expected
            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
        }

        // Private Methods
        private void InternalRead(BinaryReader reader)
        {
            Stream stream = reader.BaseStream;
            int posFileStart = (int)reader.GetPosition();

            SPR4Header header = stream.ReadStructure<SPR4Header>();

            stream.Seek(posFileStart + header.texturePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] texturePointerTable = stream.ReadStructures<TypePointerTable>(header.numTextures);

            stream.Seek(posFileStart + header.keyFramePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] keyFramePointerTable = stream.ReadStructures<TypePointerTable>(header.numKeyFrames);

            _textures = new List<TGAFile>(header.numTextures);
            for (int i = 0; i < header.numTextures; i++)
            {
                stream.Seek(posFileStart + texturePointerTable[i].offset, SeekOrigin.Begin);
                _textures.Add(new TGAFile(stream, true));
            }

            _keyFrames = new List<SPRKeyFrame>(header.numKeyFrames);
            for (int i = 0; i < header.numKeyFrames; i++)
            {
                stream.Seek(posFileStart + keyFramePointerTable[i].offset, SeekOrigin.Begin);
                _keyFrames.Add(new SPRKeyFrame(reader));
            }
        }
    }
}

