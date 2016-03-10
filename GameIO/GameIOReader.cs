namespace GameIO
{
    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;

    public class GameIOReader : BinaryReader
    {
        public Endian Endian { get; set; }

        #region Constructors

        public GameIOReader(Stream input, Endian endian)
            : base(input)
        {
            Endian = endian;
        }

        public GameIOReader(Stream input, Encoding encoding, Endian endian)
            : base(input, encoding)
        {
            Endian = endian;
        }

        public GameIOReader(Stream input, Encoding encoding, bool leaveOpen, Endian endian)
            : base(input, encoding, leaveOpen)
        {
            Endian = endian;
        }

        public GameIOReader(string filepath, Encoding encoding, bool leaveOpen, Endian endian)
            : base(File.OpenRead(filepath), encoding, leaveOpen)
        {
            Endian = endian;
        }

        public GameIOReader(byte[] data, Encoding encoding, bool leaveOpen, Endian endian)
            : base(new MemoryStream(data))
        {
            Endian = endian;
        }

        #endregion

        #region Overrides

        public override double ReadDouble()
        {
            double value = base.ReadDouble();

            if (Endian == Endian.Little)
                return value;
            else
            {
                unsafe
                {
                    byte[] bytes = BitConverter.GetBytes(value);
                    Array.Reverse(bytes);
                    return BitConverter.ToDouble(bytes, 0);
                }
            }
        }

        public override short ReadInt16()
        {
            short value = base.ReadInt16();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt16(bytes, 0);
            }
        }

        public override int ReadInt32()
        {
            int value = base.ReadInt32();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
        }

        public override long ReadInt64()
        {
            long value = base.ReadInt64();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToInt64(bytes, 0);
            }
        }

        public override float ReadSingle()
        {
            float value = base.ReadSingle();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToSingle(bytes, 0);

            }
        }

        public override ushort ReadUInt16()
        {
            ushort value = base.ReadUInt16();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToUInt16(bytes, 0);
            }
        }

        public override uint ReadUInt32()
        {
            uint value = base.ReadUInt32();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }

        public override ulong ReadUInt64()
        {
            ulong value = base.ReadUInt64();

            if (Endian == Endian.Little)
                return value;
            else
            {
                byte[] bytes = BitConverter.GetBytes(value);
                Array.Reverse(bytes);
                return BitConverter.ToUInt64(bytes, 0);
            }
        }

        #endregion Overrides

        #region String read methods

        public string ReadString(StringType type, int length = 0)
        {
            switch (type)
            {
                case StringType.NullTerminated:
                    return InternalReadNullTerminatedString();

                case StringType.FixedLength:
                    return InternalReadFixedLengthString(length);

                case StringType.PrefixedLengthByte:
                    return InternalReadPrefixedLengthByteString();

                case StringType.PrefixedLengthShort:
                    return InternalReadPrefixedLengthByteString();

                case StringType.PrefixedLengthInt:
                    return InternalReadPrefixedLengthByteString();

                default:
                    throw new ArgumentException("type");
            }
        }

        public string ReadString(StringType type, long offset, int length = 0)
        {
            string value = string.Empty;
            long originalOffset = BaseStream.Position;

            Seek(offset, SeekOrigin.Begin);
            {
                value = ReadString(type, length);
            }
            Seek(originalOffset, SeekOrigin.Begin);

            return value;
        }

        #endregion

        #region Structure reading methods

        public T ReadStructure<T>()
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize];

            if (Read(buffer, 0, structureSize) != structureSize)
            {
                throw new EndOfStreamException("could not read all of data for structure");
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return structure;
        }

        public T ReadStructure<T>(int size)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[Math.Max(structureSize, size)];

            if (Read(buffer, 0, size) != size)
            {
                throw new EndOfStreamException("could not read all of data for structure");
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return structure;
        }

        public T[] ReadStructures<T>(int count)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize * count];

            if (Read(buffer, 0, structureSize * count) != structureSize * count)
            {
                throw new EndOfStreamException("could not read all of data for structures");
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T[] structArray = new T[count];

            IntPtr bufferPtr = handle.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                structArray[i] = (T)Marshal.PtrToStructure(bufferPtr, typeof(T));
                bufferPtr += structureSize;
            }

            handle.Free();

            return structArray;
        }

        #endregion

        #region Misc reading helpers

        public long Position()
        {
            return BaseStream.Position;
        }

        public void Seek(long position, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    BaseStream.Position = position;
                    break;
                case SeekOrigin.Current:
                    BaseStream.Position += position;
                    break;
                case SeekOrigin.End:
                    BaseStream.Position = BaseStream.Length - position;
                    break;
            }
        }

        public void AlignPosition(int alignment)
        {
            BaseStream.Position = AlignmentHelper.Align(BaseStream.Position, alignment);
        }

        #endregion

        #region Internal methods

        private string InternalReadNullTerminatedString()
        {
            string value = string.Empty;
            char b = char.MinValue;

            while (true)
            {
                b = ReadChar();

                if (!(b == char.MinValue))
                    value += b;
                else
                    break;
            }

            return value;
        }

        private string InternalReadFixedLengthString(int length)
        {
            return new string(ReadChars(length));
        }

        private string InternalReadPrefixedLengthByteString()
        {
            return new string(ReadChars(ReadByte()));
        }

        private string InternalReadPrefixedLengthShortString()
        {
            return new string(ReadChars(ReadInt16()));
        }

        private string InternalReadPrefixedLengthIntString()
        {
            return new string(ReadChars(ReadInt32()));
        }

        #endregion
    }
}
