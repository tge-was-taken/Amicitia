namespace AtlusLibSharp.Graphics.RenderWare
{
    using OpenTK;
    using System;
    using System.IO;
    using AtlusLibSharp.Utilities;

    /// <summary>
    /// Represents a RenderWare bounding sphere used for collision and culling calculations.
    /// </summary>
    public struct RWBoundingSphere
    {
        private const int POS_FLAG = 1;
        private const int NRM_FLAG = 1;

        /// <summary>
        /// The <see cref="RWBoundingSphere"/> center vector.
        /// </summary>
        public readonly Vector3 Center;

        /// <summary>
        /// The <see cref="RWBoundingSphere"/> sphere radius.
        /// </summary>
        public readonly float Radius;

        /// <summary>
        /// Initialize a <see cref="RWBoundingSphere"/> using a given sphere center vector and radius.
        /// </summary>
        /// <param name="sphereCentre">The sphere center vector.</param>
        /// <param name="sphereRadius">The sphere radius.</param>
        public RWBoundingSphere(Vector3 sphereCentre, float sphereRadius)
        {
            Center = sphereCentre;
            Radius = sphereRadius;
        }

        /// <summary>
        /// Initialize a <see cref="RWBoundingSphere"/> by reading the structure from a stream using the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> used to read from the stream.</param>
        internal RWBoundingSphere(BinaryReader reader)
        {
            Center = reader.ReadVector3();
            Radius = reader.ReadSingle();
            int positionFlag = reader.ReadInt32();
            int normalFlag = reader.ReadInt32();

            /*
            if (positionFlag != POS_FLAG || normalFlag != NRM_FLAG) // maybe overzealous
            {
                throw new InvalidDataException("Position and normal flags are not set to 1");
            }
            */
        }

        /// <summary>
        /// Calculate a <see cref="RWBoundingSphere"/> using the given vertices.
        /// </summary>
        /// <param name="vertices">The vertices used in calculation.</param>
        /// <returns>A calculated <see cref="RWBoundingSphere"/></returns>
        public static RWBoundingSphere Calculate(Vector3[] vertices)
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

            return new RWBoundingSphere(sphereCentre, sphereRadius);
        }

        /// <summary>
        /// Write the <see cref="RWBoundingSphere"/> to a stream using the provided <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(Center);
            writer.Write(Radius);
            writer.Write(POS_FLAG);
            writer.Write(NRM_FLAG);
        }
    }
}