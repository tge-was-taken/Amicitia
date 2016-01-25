using System;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public struct RWTriangle
    {
        public ushort C { get; set; }
        public ushort B { get; set; }
        public ushort MatID { get; set; }
        public ushort A { get; set; }

        internal RWTriangle(BinaryReader reader)
        {
            C = reader.ReadUInt16();
            B = reader.ReadUInt16();
            MatID = reader.ReadUInt16();
            A = reader.ReadUInt16();
        }

        public RWTriangle(ushort a, ushort b, ushort c, ushort matID)
        {
            A = a;
            B = b;
            C = c;
            MatID = matID;
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(C);
            writer.Write(B);
            writer.Write(MatID);
            writer.Write(A);
        }

        public ushort this[int index]
        {
            get
            {
                if (index == 0)
                    return A;
                else if (index == 1)
                    return B;
                else if (index == 2)
                    return C;
                else
                    throw new ArgumentOutOfRangeException();
            }
            set
            {
                if (index == 0)
                    A = value;
                else if (index == 1)
                    B = value;
                else if (index == 2)
                    C = value;
                else
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}