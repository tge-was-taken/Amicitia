namespace AtlusLibSharp.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Provide extensions for the BinaryReader and BinaryWriter to in order to write more flexible code.
    /// </summary>
    internal static class BinaryIOExtension
    {
        private static Encoding _sjisEncoding = Encoding.GetEncoding("Shift_JIS");

        // Get/Set Position extensions
        public static long GetPosition(this BinaryReader reader)
        {
            return reader.BaseStream.Position;
        }

        public static void SetPosition(this BinaryReader reader, long position)
        {
            reader.BaseStream.Position = position;
        }

        public static long GetPosition(this BinaryWriter writer)
        {
            return writer.BaseStream.Position;
        }

        public static void SetPosition(this BinaryWriter writer, long position)
        {
            writer.BaseStream.Position = position;
        }

        // Seek extensions
        public static void Seek(this BinaryReader reader, long position, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    reader.BaseStream.Position = position;
                    break;
                case SeekOrigin.Current:
                    reader.BaseStream.Position += position;
                    break;
                case SeekOrigin.End:
                    reader.BaseStream.Position = reader.BaseStream.Length - position;
                    break;
            }
        }

        public static void Seek(this BinaryWriter writer, long position, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    writer.BaseStream.Position = position;
                    break;
                case SeekOrigin.Current:
                    writer.BaseStream.Position += position;
                    break;
                case SeekOrigin.End:
                    writer.BaseStream.Position = writer.BaseStream.Length - position;
                    break;
            }
        }

        // Alignment extensions
        public static void AlignPosition(this BinaryReader reader, int alignmentBytes)
        {
            reader.BaseStream.Position = AlignmentHelper.Align(reader.BaseStream.Position, alignmentBytes);
        }

        public static void AlignPosition(this BinaryWriter writer, int alignmentBytes)
        {
            long align = AlignmentHelper.Align(writer.BaseStream.Position, alignmentBytes);
            writer.Write(new byte[(align - writer.BaseStream.Position)]);
        }

        // Write extensions
        public static void Write(this BinaryWriter writer, System.Drawing.Color value)
        {
            writer.Write(value.ToArgb());
        }

        public static void Write(this BinaryWriter writer, System.Drawing.Color[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public static void Write(this BinaryWriter writer, Vector3[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public static void Write(this BinaryWriter writer, Vector2[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        public static void Write(this BinaryWriter writer, Matrix4x4 value, bool asMatrix3x4)
        {
            const int numRow = 4;
            int numElem = 4;
            if (asMatrix3x4)
                numElem--;

            for (int i = 0; i < numRow; i++)
            {
                for (int j = 0; j < numElem; j++)
                {
                    writer.Write(value.GetElementAt(i, j));
                }
            }
        }

        public static void Write(this BinaryWriter writer, Matrix4x4[] array, bool asMatrix3x4)
        {
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i], asMatrix3x4);
            }
        }

        public static void WriteCString(this BinaryWriter writer, string value)
        {
            writer.Write(_sjisEncoding.GetBytes(value));
            writer.Write((byte)0);
        }

        public static void WriteCString(this BinaryWriter writer, string value, int length)
        {
            if (value.Length > length)
            {
                throw new ArgumentException();
            }

            writer.Write(_sjisEncoding.GetBytes(value));
            writer.Write(new byte[length - _sjisEncoding.GetByteCount(value)]);
        }

        public static void WriteCStringAligned(this BinaryWriter writer, string value)
        {
            writer.Write(_sjisEncoding.GetBytes(value));
            writer.AlignPosition(4);
        }

        // Write array extensions
        public static void Write(this BinaryWriter writer, byte value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                writer.Write(value);
            }
        }

        public static void Write(this BinaryWriter writer, byte[] value, long offset)
        {
            long posStart = writer.GetPosition();
            writer.Seek(offset, SeekOrigin.Begin);
            writer.Write(value);
            writer.Seek(posStart, SeekOrigin.Begin);
        }

        public static void Write(this BinaryWriter writer, short[] value)
        {
            foreach (short item in value)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, ushort[] value)
        {
            foreach (ushort item in value)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, int[] value)
        {
            foreach (int item in value)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, uint[] value)
        {
            foreach (uint item in value)
            {
                writer.Write(item);
            }
        }

        public static void Write(this BinaryWriter writer, float[] value)
        {
            foreach (float item in value)
            {
                writer.Write(item);
            }
        }

        // Read extensions
        public static System.Drawing.Color ReadColor(this BinaryReader reader)
        {
            int color = reader.ReadInt32();
            return System.Drawing.Color.FromArgb(color & 0xFF, (color >> 24) & 0xFF, (color >> 16) & 0xFF, (color >> 8) & 0xFF);
            //return System.Drawing.Color.FromArgb(reader.ReadInt32());
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Matrix4x4 ReadMatrix4x3(this BinaryReader reader)
        {
            float[] val = reader.ReadFloatArray(4 * 3);
            return new Matrix4x4(val[0], val[1], val[2], 0, val[3], val[4], val[5], 0, val[6], val[7], val[8], 0, val[9], val[10], val[11], 1);
        }

        public static Matrix4x4 ReadMatrix4(this BinaryReader reader)
        {
            float[] val = reader.ReadFloatArray(4 * 4);
            return new Matrix4x4(val[0], val[1], val[2], val[3], val[4], val[5], val[6], val[7], val[8], val[9], val[10], val[11], val[12], val[13], val[14], val[15]);
        }

        public static string ReadCString(this BinaryReader reader)
        {
            List<byte> bytes = new List<byte>();
            byte b = reader.ReadByte();
            while (b != 0)
            {
                bytes.Add(b);
                b = reader.ReadByte();
            }
            return _sjisEncoding.GetString(bytes.ToArray());
        }

        public static string ReadCString(this BinaryReader reader, int length)
        {
            return _sjisEncoding.GetString(reader.ReadBytes(length)).Trim('\0');
        }

        public static string ReadCStringAtOffset(this BinaryReader reader, int offset)
        {
            long posStart = reader.GetPosition();
            reader.Seek(offset, SeekOrigin.Begin);
            string str = reader.ReadCString();
            reader.Seek(posStart, SeekOrigin.Begin);
            return str;
        }

        public static string ReadCStringAligned(this BinaryReader reader)
        {
            string str = reader.ReadCString();
            reader.AlignPosition(4);
            return str;
        }

        // Read array extensions
        public static byte[] ReadBytesAtOffset(this BinaryReader reader, int count, long offset)
        {
            long returnPosition = reader.BaseStream.Position;
            reader.Seek(offset, SeekOrigin.Begin);
            byte[] data = reader.ReadBytes(count);
            reader.Seek(returnPosition, SeekOrigin.Begin);
            return data;
        }

        public static short[] ReadInt16Array(this BinaryReader reader, int count)
        {
            short[] arr = new short[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadInt16();
            }

            return arr;
        }

        public static ushort[] ReadUInt16Array(this BinaryReader reader, int count)
        {
            ushort[] arr = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadUInt16();
            }

            return arr;
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int count)
        {
            int[] arr = new int[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadInt32();
            }

            return arr;
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int count)
        {
            uint[] arr = new uint[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadUInt32();
            }

            return arr;
        }

        public static float[] ReadFloatArray(this BinaryReader reader, int count)
        {
            float[] arr = new float[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadSingle();
            }

            return arr;
        }

        public static System.Drawing.Color[] ReadColorArray(this BinaryReader reader, int count)
        {
            System.Drawing.Color[] arr = new System.Drawing.Color[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadColor();
            }

            return arr;
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader, int count)
        {
            Vector2[] arr = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadVector2();
            }

            return arr;
        }

        public static Vector3[] ReadVector3Array(this BinaryReader reader, int count)
        {
            Vector3[] arr = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadVector3();
            }

            return arr;
        }

        public static Matrix4x4[] ReadMatrix4x3Array(this BinaryReader reader, int count)
        {
            Matrix4x4[] arr = new Matrix4x4[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadMatrix4x3();
            }

            return arr;
        }

        public static Matrix4x4[] ReadMatrix4Array(this BinaryReader reader, int count)
        {
            Matrix4x4[] arr = new Matrix4x4[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.ReadMatrix4();
            }

            return arr;
        }

        public static string[] ReadCStringArray(this BinaryReader reader, int count, int length = -1)
        {
            string[] arr = new string[count];
            for (int i = 0; i < count; i++)
            {
                if (length != -1)
                {
                    arr[i] = reader.ReadCString(length);
                }
                else
                {
                    arr[i] = reader.ReadCString();
                }
            }

            return arr;
        }

        // Structure extensions

        public static T ReadStructure<T>(this BinaryReader reader)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize];

            if (reader.Read(buffer, 0, structureSize) != structureSize)
            {
                throw new EndOfStreamException("could not read all of data for structure");
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return structure;
        }

        public static T ReadStructure<T>(this BinaryReader reader, int size)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[Math.Max(structureSize, size)];

            if (reader.Read(buffer, 0, size) != size)
            {
                throw new EndOfStreamException("could not read all of data for structure");
            }

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            T structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

            handle.Free();

            return structure;
        }

        public static T[] ReadStructures<T>(this BinaryReader reader, int count)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize * count];

            if (reader.Read(buffer, 0, structureSize * count) != structureSize * count)
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

        public static void WriteStructure<T>(this BinaryWriter writer, T structure)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);

            handle.Free();

            writer.Write(buffer, 0, buffer.Length);
        }

        public static void WriteStructure<T>(this BinaryWriter writer, T structure, int size)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[Math.Max(structureSize, size)];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);

            handle.Free();

            writer.Write(buffer, 0, buffer.Length);
        }

        public static void WriteStructures<T>(this BinaryWriter writer, T[] structArray, int count)
        {
            int structureSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[structureSize * count];
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPtr = handle.AddrOfPinnedObject();

            for (int i = 0; i < count; i++)
            {
                Marshal.StructureToPtr(structArray[i], bufferPtr, false);
                bufferPtr += structureSize;
            }

            handle.Free();

            writer.Write(buffer, 0, buffer.Length);
        }
    }
}
