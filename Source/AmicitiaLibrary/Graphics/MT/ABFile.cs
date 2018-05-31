namespace AmicitiaLibrary.Graphics.MT
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using AmicitiaLibrary.Utilities;
    using IO;
    using Compression;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    internal struct MT00Header
    {
        internal const short FLAG = 0x0008;
        internal const string TAG = "MT00";
        public const byte SIZE = 0x28;

        [FieldOffset(0)]
        public short flag;

        [FieldOffset(2)]
        public short userID;

        [FieldOffset(4)]
        public int length;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4+1)]
        public string tag;

        [FieldOffset(12)]
        public int unused;

        [FieldOffset(16)]
        public int addressRelocTableOffset;

        [FieldOffset(20)]
        public int addressRelocTableSize;

        [FieldOffset(24)]
        public short numUnknown1;

        [FieldOffset(26)]
        public short numUnknown2;

        [FieldOffset(28)]
        public short numUnknown3;

        [FieldOffset(30)]
        public short unknown4;

        [FieldOffset(32)]
        public short numAnims;

        [FieldOffset(34)]
        public short numKeys;

        [FieldOffset(36)]
        public int animPointerTableOffset;

        public MT00Header(int animCount, int keyCount)
        {
            flag = FLAG;
            userID = 0;
            length = 0;
            tag = TAG;
            unused = 0;
            addressRelocTableOffset = 0;
            addressRelocTableSize = 0;
            numUnknown1 = 0;
            numUnknown2 = 0;
            numUnknown3 = 0;
            unknown4 = 0;
            numAnims = (short)animCount;
            numKeys = (short)keyCount;
            animPointerTableOffset = 0;
        }
    }

    public class AbFile : BinaryBase
    {
        internal const int DATA_START_ADDRESS = 0x20;

        // Private fields
        private AbKeyInfo[] mAnimKeyInfo;
        private AbAnimation[] mAnims;

        // Constructors
        internal AbFile(BinaryReader reader)
        {
            Read(reader);
        }

        // Properties
        public int AnimationCount
        {
            get { return mAnims.Length; }
        }

        public int KeyCount
        {
            get { return mAnimKeyInfo.Length; }
        }

        public AbKeyInfo[] KeyInfoArray
        {
            get { return mAnimKeyInfo; }
        }

        public AbAnimation[] Animations
        {
            get { return mAnims; }
        }

        // Public Static Methods
        public static AbFile LoadFrom(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                return new AbFile(reader);
        }

        public static AbFile LoadFrom(Stream stream, bool leaveStreamOpen)
        {
            using (BinaryReader reader = new BinaryReader(stream, System.Text.Encoding.Default, leaveStreamOpen))
                return new AbFile(reader);
        }

        // Methods
        internal override void Write(BinaryWriter writer)
        {
            // Skip header
            long posFileStart = (int)writer.BaseStream.Position;
            writer.Seek(MT00Header.SIZE, SeekOrigin.Current);

            MT00Header header = new MT00Header(AnimationCount, KeyCount);

            // Create address list for reloc table
            List<int> addressList = new List<int>();

            // Calculate size of key info table & anim pointer table offset
            int keyInfoTableSize = header.numKeys * AbKeyInfo.SIZE;
            header.animPointerTableOffset = (int)(((writer.GetPosition() + keyInfoTableSize) - DATA_START_ADDRESS) - posFileStart);

            addressList.Add((int)(writer.GetPosition() - 4 - posFileStart)); // animPointerTableOffset

            // Write key info
            for (int i = 0; i < mAnimKeyInfo.Length; i++)
            {
                writer.Write((ushort)mAnimKeyInfo[i].KeyType);
                writer.Write(mAnimKeyInfo[i].UnkMorph);
                writer.Write(mAnimKeyInfo[i].BoneIndex);
            }

            // Calculate anim pointer table size
            long posAnimPointerTable = writer.GetPosition();
            int animPointerTableSize = header.numAnims * sizeof(int);
            int animPointerBase = (int)posAnimPointerTable + animPointerTableSize;

            // Write anims
            int[] animPointers = new int[header.numAnims];
            for (int i = 0; i < animPointers.Length; i++)
            {
                if (mAnims[i] == null)
                {
                    animPointers[i] = 0;
                }
                else
                {
                    animPointers[i] = (int)((animPointerBase - DATA_START_ADDRESS) - posFileStart);

                    writer.Seek(animPointerBase, SeekOrigin.Begin);
                    mAnims[i].Write(writer);
                    animPointerBase = (int)writer.GetPosition();
                }
            }

            // Write pointer table
            writer.Seek(posAnimPointerTable, SeekOrigin.Begin);
            for (int i = 0; i < animPointers.Length; i++)
            {
                if (mAnims[i] != null)
                {
                    addressList.Add((int)(writer.GetPosition() - posFileStart));
                    writer.Write(animPointers[i]);
                }
            }

            writer.Seek(animPointerBase, SeekOrigin.Begin);

            // Calculate address reloc table values
            byte[] addressRelocTable = PointerRelocationTableCompression.Compress(addressList, DATA_START_ADDRESS);
            header.addressRelocTableSize = addressRelocTable.Length;
            header.addressRelocTableOffset = (int)((writer.GetPosition() - DATA_START_ADDRESS) - posFileStart);

            // Write address reloc table
            writer.Write(addressRelocTable);

            long posFileEnd = writer.GetPosition();

            // Calculate length
            header.length = (int)(posFileEnd - posFileStart);

            // Write header
            writer.Seek(posFileStart, SeekOrigin.Begin);
            writer.WriteStructure(header);

            writer.BaseStream.Seek(posFileEnd, SeekOrigin.Begin);
            writer.AlignPosition(64);
        }

        internal void Read(BinaryReader reader)
        {
            int posFileStart = (int)reader.BaseStream.Position;
            MT00Header header = reader.ReadStructure<MT00Header>();

            mAnimKeyInfo = reader.ReadStructures<AbKeyInfo>(header.numKeys);

            reader.BaseStream.Seek(posFileStart + DATA_START_ADDRESS + header.animPointerTableOffset, SeekOrigin.Begin);
            int[] animPointers = reader.ReadInt32Array(header.numAnims);

            mAnims = new AbAnimation[header.numAnims];
            for (int i = 0; i < mAnims.Length; i++)
            {
                if (animPointers[i] == 0)
                {
                    // Some pointers just serve as a dummy animation slot
                    continue;
                }

                reader.BaseStream.Seek(posFileStart + DATA_START_ADDRESS + animPointers[i], SeekOrigin.Begin);
                mAnims[i] = new AbAnimation(header.numKeys, reader);
            }
        }
    }
}
