namespace AtlusLibSharp.Graphics.RenderWare
{
    using System;
    using System.IO;

    /// <summary>
    /// A RenderWare triangle: Vertex indices that together form a triangle along with the specified material index for that triangle.
    /// </summary>
    public struct RWTriangle
    {
        /// <summary>
        /// Index of the third vertex.
        /// </summary>
        public ushort C;

        /// <summary>
        /// Index of the second vertex.
        /// </summary>
        public ushort B;

        /// <summary>
        /// Index of the material applied to this triangle.
        /// </summary>
        public short MatID;

        /// <summary>
        /// Index of the first vertex.
        /// </summary>
        public ushort A;

        /// <summary>
        /// Initialize an instance of this structure using given parameters.
        /// </summary>
        /// <param name="a">First vertex index.</param>
        /// <param name="b">Second vertex index.</param>
        /// <param name="c">Third vertex index.</param>
        /// <param name="matID">Material index.</param>
        public RWTriangle(ushort a, ushort b, ushort c, short matID)
        {
            A = a;
            B = b;
            C = c;
            MatID = matID;
        }

        /// <summary>
        /// Initialize an instance of this structure by reading it from the given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> to read the structure from.</param>
        internal RWTriangle(BinaryReader reader)
        {
            C = reader.ReadUInt16();
            B = reader.ReadUInt16();
            MatID = reader.ReadInt16();
            A = reader.ReadUInt16();
        }

        /// <summary>
        /// Gets or sets a value using a given index.
        /// </summary>
        /// <param name="index">Index indicating the nth vertex index in this triangle.</param>
        /// <returns></returns>
        public ushort this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return A;
                    case 1:
                        return B;
                    case 2:
                        return C;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        A = value;
                        break;
                    case 1:
                        B = value;
                        break;
                    case 2:
                        C = value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Write the instance to the given <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the instance to.</param>
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(C);
            writer.Write(B);
            writer.Write(MatID);
            writer.Write(A);
        }
    }
}