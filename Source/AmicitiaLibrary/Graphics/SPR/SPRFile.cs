using System.Text;

namespace AmicitiaLibrary.Graphics.SPR
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using Utilities;
    using IO;
    using System.Linq;
    using TMX;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct Spr0Header
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

        public Spr0Header(int textureCount, int keyFrameCount)
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

    public abstract class SprFileBase : BinaryBase
    {
        protected List<SprKeyFrame> keyFrames;
        protected List<ITextureFile> textures;

        internal abstract string Tag
        {
            get;
        }

        public abstract List<SprKeyFrame> KeyFrames
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

    public class SprFile : BinaryBase, ISprFile
    {
        // Private Fields
        private IList<TmxFile> mTextures;
        private IList<SprKeyFrame> mKeyFrames;

        // Constructors
        internal SprFile(BinaryReader reader)
        {
            Read(reader);
        }

        public SprFile(IList<TmxFile> textures, IList<SprKeyFrame> keyframes)
        {
            mTextures = textures.ToList();
            mKeyFrames = keyframes.ToList();
        }

        public SprFile(Stream stream, bool leaveOpen = false)
        {
            using (var reader = new BinaryReader(stream, Encoding.Default, leaveOpen))
                Read(reader);
        }

        // Properties
        public int TextureCount
        {
            get { return mTextures.Count; }
        }

        public int KeyFrameCount
        {
            get { return mKeyFrames.Count; }
        }

        public IList<TmxFile> Textures
        {
            get { return mTextures; }
        }

        public IList<SprKeyFrame> KeyFrames
        {
            get { return mKeyFrames; }
        }

        IList<ITextureFile> ISprFile.Textures => mTextures.Cast<ITextureFile>().ToList();

        // Public Methods
        public static SprFile Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path), System.Text.Encoding.Default, true))
                return new SprFile(reader);
        }

        public static SprFile LoadFrom(Stream stream, bool leaveStreamOpen = false)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new SprFile(reader);
        }

        // Internal Methods
        internal override void Write(BinaryWriter writer)
        {
            // Save the start position to calculate the filesize and 
            // to write out the header after we know where all the structure offsets are
            Stream stream = writer.BaseStream;
            int posFileStart = (int)stream.Position;
            stream.Seek(Spr0Header.SIZE, SeekOrigin.Current);

            // Create initial header and tables
            Spr0Header header = new Spr0Header(TextureCount, KeyFrameCount);
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
                mKeyFrames[i].Write(writer);
            }

            writer.Seek(16, SeekOrigin.Current);
            writer.AlignPosition(16);

            // Write out the texture data and fill up the pointer table
            for (int i = 0; i < header.numTextures; i++)
            {
                texturePointerTable[i].offset = (int)(stream.Position - posFileStart);
                mTextures[i].Write(writer);
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
        private void Read(BinaryReader reader)
        {
            Stream stream = reader.BaseStream;
            int posFileStart = (int)reader.GetPosition();

            Spr0Header header = stream.ReadStructure<Spr0Header>();

            stream.Seek(posFileStart + header.texturePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] texturePointerTable = stream.ReadStructures<TypePointerTable>(header.numTextures);

            stream.Seek(posFileStart + header.keyFramePointerTableOffset, SeekOrigin.Begin);
            TypePointerTable[] keyFramePointerTable = stream.ReadStructures<TypePointerTable>(header.numKeyFrames);

            mTextures = new List<TmxFile>(header.numTextures);
            for (int i = 0; i < header.numTextures; i++)
            {
                stream.Seek(posFileStart + texturePointerTable[i].offset, SeekOrigin.Begin);
                mTextures.Add(TmxFile.Load(stream, true));
            }

            mKeyFrames = new List<SprKeyFrame>(header.numKeyFrames);
            for (int i = 0; i < header.numKeyFrames; i++)
            {
                stream.Seek(posFileStart + keyFramePointerTable[i].offset, SeekOrigin.Begin);
                mKeyFrames.Add(new SprKeyFrame(reader));
            }
        }
    }
}
