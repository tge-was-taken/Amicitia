namespace AtlusLibSharp.Graphics.SPR
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using Utilities;
    using IO;
    using System.Linq;
    using TMX;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct SPR0Header
    {
        public const ushort FLAGS = 0x0001;
        public const string TAG = "SPR0";
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

        public SPR0Header(int textureCount, int keyFrameCount)
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

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct TypePointerTable
    {
        public const int SIZE = 0x08;

        [FieldOffset(0)]
        public int type;

        [FieldOffset(4)]
        public int offset;
    }

    public abstract class SPRFileBase : BinaryFileBase
    {
        protected List<SPRKeyFrame> keyFrames;
        protected List<ITextureFile> textures;

        internal abstract string Tag
        {
            get;
        }

        public abstract List<SPRKeyFrame> KeyFrames
        {
            get;
        }

        public abstract List<ITextureFile> Textures
        {
            get;
        }

        public int TextureCount
        {
            get { return KeyFrames.Count; }
        }

        public int KeyFrameCount
        {
            get { return KeyFrames.Count; }
        }
    }

    public class SPRFile : BinaryFileBase
    {
        // Private Fields
        private List<TMXFile> _textures;
        private List<SPRKeyFrame> _keyFrames;

        // Constructors
        internal SPRFile(BinaryReader reader)
        {
            InternalRead(reader);
        }

        public SPRFile(IList<TMXFile> textures, IList<SPRKeyFrame> keyframes)
        {
            _textures = textures.ToList();
            _keyFrames = keyframes.ToList();
        }

        // Properties
        public int TextureCount
        {
            get { return _textures.Count; }
        }

        public int KeyFrameCount
        {
            get { return _keyFrames.Count; }
        }

        public List<TMXFile> Textures
        {
            get { return _textures; }
        }

        public List<SPRKeyFrame> KeyFrames
        {
            get { return _keyFrames; }
        }

        // Public Methods
        public static SPRFile Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default, true))
                return new SPRFile(reader);
        }

        public static SPRFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new SPRFile(reader);
        }

        // Internal Methods
        internal override void InternalWrite(BinaryWriter writer)
        {
            // Save the start position to calculate the filesize and 
            // to write out the header after we know where all the structure offsets are
            Stream stream = writer.BaseStream;
            int posFileStart = (int)stream.Position;
            stream.Seek(SPR0Header.SIZE, SeekOrigin.Current);

            // Create initial header and tables
            SPR0Header header = new SPR0Header(TextureCount, KeyFrameCount);
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
                //writer.AlignPosition(16);
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

            SPR0Header header = stream.ReadStructure<SPR0Header>();

            stream.Seek(posFileStart + header.texturePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] texturePointerTable = stream.ReadStructures<TypePointerTable>(header.numTextures);

            stream.Seek(posFileStart + header.keyFramePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] keyFramePointerTable = stream.ReadStructures<TypePointerTable>(header.numKeyFrames);

            _textures = new List<TMXFile>(header.numTextures);
            for (int i = 0; i < header.numTextures; i++)
            {
                stream.Seek(posFileStart + texturePointerTable[i].offset, SeekOrigin.Begin);
                _textures.Add(TMXFile.Load(stream, true));
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
