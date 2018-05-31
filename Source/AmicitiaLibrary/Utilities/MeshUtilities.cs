namespace AmicitiaLibrary.Utilities
{
    using System.Numerics;
    using System;
    using System.Collections.Generic;

    public static class MeshUtilities
    {
        public static Vector3[] CalculateAverageNormals<T>(IList<IList<T>> triangleIndices, IList<Vector3> positions)
        {
            Vector3[] normals = new Vector3[positions.Count];

            for (int i = 0; i < triangleIndices.Count; i++)
            {
                Vector3 p1 = positions[Convert.ToInt32(triangleIndices[i][0])];
                Vector3 p2 = positions[Convert.ToInt32(triangleIndices[i][1])];
                Vector3 p3 = positions[Convert.ToInt32(triangleIndices[i][2])];

                Vector3 v1 = p2 - p1;
                Vector3 v2 = p3 - p1;
                Vector3 normal = Vector3.Cross(v1, v2);

                if (normal != Vector3.Zero)
                {
                    normal = Vector3.Normalize(normal);
                }

                // Store the face's normal for each of the vertices that make up the face.
                normals[Convert.ToInt32(triangleIndices[i][0])] += normal;
                normals[Convert.ToInt32(triangleIndices[i][1])] += normal;
                normals[Convert.ToInt32(triangleIndices[i][2])] += normal;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                if (normals[i] != Vector3.Zero)
                {
                    normals[i] = Vector3.Normalize(normals[i]);
                }
            }

            return normals;
        }

        public static int[] ToTriangleList(IReadOnlyList<int> triangleStrips, bool flip = true)
        {
            List<int> triangles = new List<int>();
            int numTris = triangleStrips.Count - 2;

            int fa = triangleStrips[0];
            int fb = triangleStrips[1];
            int fc = 0;

            for (int i = 2; i < triangleStrips.Count; i++)
            {
                fc = triangleStrips[i];
                flip = !flip;

                // check if tri is degenerate
                if ((fa != fb) && (fb != fc) && (fc != fa))
                {
                    if (flip)
                        triangles.AddRange(fa, fb, fc);
                    else
                        triangles.AddRange(fa, fc, fb);
                }

                fa = fb;
                fb = fc;
            }

            return triangles.ToArray();
        }
    }
}
