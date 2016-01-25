namespace AtlusLibSharp.SMT3.ChunkResources
{
    // System
    using System;
    using System.Collections.Generic;
    using System.IO;

    // AtlusLibSharp
    using Utilities;
    using Animation;
    using Graphics;
    using Modeling;
    using Scripting;

    /// <summary>
    /// Class capable of reading and creating Chunks from files.
    /// </summary>
    public static class ChunkFactory
    {
        /// <summary>
        /// Get a list of chunks read from the provided stream.
        /// </summary>
        public static List<Chunk> Get(Stream stream, bool singleMode = false)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("The input stream is null");
            }

            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Get(reader, singleMode);
            }
        }

        /// <summary>
        /// Get a list of chunks read from the file at the provided path.
        /// </summary>
        public static List<Chunk> Get(string path, bool singleMode = false)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return Get(File.OpenRead(path), singleMode);
        }

        /// <summary>
        /// Get a list of chunks read from the BaseStream of the BinaryReader
        /// </summary>
        public static List<Chunk> Get(BinaryReader reader, bool singleMode = false)
        {
            if (reader.BaseStream.Position + 16 > reader.BaseStream.Length)
            {
                throw new InvalidDataException("Not enough space to read chunk from the stream");
            }

            List<Chunk> list = new List<Chunk>(16);

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                IntPtr ptr = new IntPtr(reader.BaseStream.Position);
                ushort flags = reader.ReadUInt16();
                ushort id = reader.ReadUInt16();
                int length = reader.ReadInt32();
                string magic = reader.ReadCString(4);

                switch (magic)
                {
                    case TXPChunk.TXP0_TAG:
                        list.Add(new TXPChunk(id, length, reader));
                        break;
                    case TMXChunk.TMX0_TAG:
                        list.Add(new TMXChunk(id, length, reader));
                        break;
                    case MDChunk.MD00_TAG:
                        list.Add(new MDChunk(id, length, reader));
                        break;
                    case MTChunk.MT00_TAG:
                        list.Add(new MTChunk(id, length, reader));
                        break;
                    case SPRChunk.SPR0_TAG:
                        list.Add(new SPRChunk(id, ref length /* pass length by ref to fix */, reader));
                        break;
                    case MSGChunk.MSG1_TAG:
                        list.Add(new MSGChunk(id, length, reader));
                        break;
                    case BFChunk.FLW0_TAG:
                        list.Add(new BFChunk(id, length, reader));
                        break;
                }

                reader.BaseStream.Position = (int)ptr + length;
                reader.AlignPosition(64);

                // Check if we're in single read mode
                // And return early if we are
                if (singleMode)
                {
                    return list;
                }
            }

            return list;
        }

        /// <summary>
        /// Get a chunk from the stream of the desired type.
        /// Will return null if the specified type doesn't match the actual type.
        /// </summary>
        public static T Get<T>(Stream stream)
            where T : class
        {
            return Get(stream, true)[0] as T;
        }

        /// <summary>
        /// Get a chunk from the file at the provided path of the specified type.
        /// Will return null if the specified type doesn't match the actual type.
        /// </summary>
        public static T Get<T>(string path)
            where T : class
        {
            return Get(path, true)[0] as T;
        }

        /// <summary>
        /// Get a chunk read from the BaseStream of the BinaryReader of the desired type.
        /// Will return null if the specified type doesn't match the actual type.
        /// </summary>
        public static T Get<T>(BinaryReader reader)
            where T : class
        {
            return Get(reader, true)[0] as T;
        }
    }
}
