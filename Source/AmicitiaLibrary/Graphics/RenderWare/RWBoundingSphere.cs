using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Numerics;
    using System;
    using System.IO;
    using Utilities;

    /// <summary>
    /// Represents a RenderWare bounding sphere used for collision and culling calculations.
    /// </summary>
    public class RwBoundingSphere : BinaryBase
    {
        private const int POS_FLAG = 1;
        private const int NRM_FLAG = 1;

        /// <summary>
        /// The <see cref="RwBoundingSphere"/> center vector.
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// The <see cref="RwBoundingSphere"/> sphere radius.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Initialize a <see cref="RwBoundingSphere"/> using a given sphere center vector and radius.
        /// </summary>
        /// <param name="sphereCentre">The sphere center vector.</param>
        /// <param name="sphereRadius">The sphere radius.</param>
        public RwBoundingSphere(Vector3 sphereCentre, float sphereRadius)
        {
            Center = sphereCentre;
            Radius = sphereRadius;
        }

        /// <summary>
        /// Initialize a <see cref="RwBoundingSphere"/> by reading the structure from a stream using the <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> used to read from the stream.</param>
        internal RwBoundingSphere(BinaryReader reader)
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
        /// Calculate a <see cref="RwBoundingSphere"/> using the given vertices.
        /// </summary>
        /// <param name="vertices">The vertices used in calculation.</param>
        /// <returns>A calculated <see cref="RwBoundingSphere"/></returns>
        public static RwBoundingSphere Calculate(Vector3[] vertices)
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
                maxDistSq = Math.Max(maxDistSq, fromCentre.LengthSquared());
            }

            float sphereRadius = (float)Math.Sqrt(maxDistSq);

            return new RwBoundingSphere(sphereCentre, sphereRadius);
        }

        /// <summary>
        /// Write the <see cref="RwBoundingSphere"/> to a stream using the provided <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the stream.</param>
        internal override void Write(BinaryWriter writer)
        {
            writer.Write(Center);
            writer.Write(Radius);
            writer.Write(POS_FLAG);
            writer.Write(NRM_FLAG);
        }
    }
}