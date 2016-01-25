using OpenTK;
using System;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    using Utilities;

    public struct RWBoundingInfo
    {
        // Fields
        public Vector3 Center;
        public float Radius;
        public int PositionFlag;
        public int NormalFlag;

        // Constructors
        public RWBoundingInfo(Vector3 sphereCentre, float sphereRadius)
        {
            Center = sphereCentre;
            Radius = sphereRadius;
            PositionFlag = 1;
            NormalFlag = 1;
        }

        internal RWBoundingInfo(BinaryReader reader)
        {
            Center = reader.ReadVector3();
            Radius = reader.ReadSingle();
            PositionFlag = reader.ReadInt32();
            NormalFlag = reader.ReadInt32();
        }

        // Methods
        public static RWBoundingInfo Create(Vector3[] vertices)
        {
            Vector3 minExtent = Vector3.Zero;
            Vector3 maxExtent = Vector3.Zero;
            foreach (Vector3 vertex in vertices)
            {
                minExtent.X = Math.Min(minExtent.X, vertex.X);
                minExtent.Y = Math.Min(minExtent.Y, vertex.Y);
                minExtent.Z = Math.Min(minExtent.Z, vertex.Z);

                maxExtent.X = Math.Max(maxExtent.X, vertex.X);
                maxExtent.Y = Math.Max(maxExtent.Y, vertex.Y);
                maxExtent.Z = Math.Max(maxExtent.Z, vertex.Z);
            }

            Vector3 sphereCentre = new Vector3
            {
                X = (float)0.5 * (minExtent.X + maxExtent.X),
                Y = (float)0.5 * (minExtent.Y + maxExtent.Y),
                Z = (float)0.5 * (minExtent.Z + maxExtent.Z)
            };

            float maxDistSq = 0.0f;
            foreach (Vector3 vertex in vertices)
            {
                Vector3 fromCentre = vertex - sphereCentre;
                maxDistSq = Math.Max(maxDistSq, fromCentre.LengthSquared);
            }

            float sphereRadius = (float)Math.Sqrt(maxDistSq);

            return new RWBoundingInfo(sphereCentre, sphereRadius);
        }

        internal void Write(BinaryWriter writer)
        {
            writer.Write(Center);
            writer.Write(Radius);
            writer.Write(PositionFlag);
            writer.Write(NormalFlag);
        }
    }
}