using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWGeometryStruct : RWNode
    {
        public RWGeometryFlags GeometryFlags { get; private set; }
        public byte TexCoordCount { get; private set; }
        public RWGeometryNativeFlag NativeFlag { get; set; }
        public uint FaceCount { get; private set; }
        public uint VertexCount { get; private set; }
        public uint MorphTargetCount { get; private set; } // unused, always set to 1
        public Color[] Colors { get; set; }
        public Vector2[][] TexCoordSets { get; set; }
        public RWTriangle[] Triangles { get; set; }
        public RWBoundingInfo BoundingInfo { get; private set; }
        public Vector3[] Positions { get; set; }
        public Vector3[] Normals { get; set; }

        // TODO:
        // 1. Implement support for multiple texture coords for newly generated models
        // Priority isn't very high and requires some changes to how the optimizer works

        internal RWGeometryStruct(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Struct, size, version, parent)
        {
            GeometryFlags = (RWGeometryFlags) reader.ReadUInt16();
            TexCoordCount = reader.ReadByte();
            NativeFlag = (RWGeometryNativeFlag) reader.ReadByte();
            FaceCount = reader.ReadUInt32();
            VertexCount = reader.ReadUInt32();
            MorphTargetCount = reader.ReadUInt32();

            if ((GeometryFlags & RWGeometryFlags.HasVertexColor) == RWGeometryFlags.HasVertexColor)
            {
                Colors = new Color[VertexCount];
                for (int i = 0; i < VertexCount; i++)
                    Colors[i] = Color.FromArgb(reader.ReadInt32());
            }

            if ((GeometryFlags & RWGeometryFlags.HasTexCoord1) == RWGeometryFlags.HasTexCoord1 || 
                (GeometryFlags & RWGeometryFlags.HasTexCoord2) == RWGeometryFlags.HasTexCoord2)
            {
                TexCoordSets = new Vector2[TexCoordCount][];
                for (int i = 0; i < TexCoordCount; i++)
                {
                    TexCoordSets[i] = new Vector2[VertexCount];
                    for (int j = 0; j < VertexCount; j++)
                        TexCoordSets[i][j] = new Vector2(
                            reader.ReadSingle(),
                            reader.ReadSingle());
                }
            }

            Triangles = new RWTriangle[FaceCount];
            for (int i = 0; i < FaceCount; i++)
                Triangles[i] = new RWTriangle(reader);

            BoundingInfo = new RWBoundingInfo(reader);

            if ((GeometryFlags & RWGeometryFlags.HasVertexPosition) == RWGeometryFlags.HasVertexPosition)
            {
                Positions = new Vector3[VertexCount];
                for (int i = 0; i < VertexCount; i++)
                    Positions[i] = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
            }

            if ((GeometryFlags & RWGeometryFlags.HasVertexNormal) == RWGeometryFlags.HasVertexNormal)
            {
                Normals = new Vector3[VertexCount];
                for (int i = 0; i < VertexCount; i++)
                    Normals[i] = new Vector3(
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle());
            }
        }

        public RWGeometryStruct(Vector3[] pos, Vector3[] nrm, ushort[] matIds, Vector2[][] uvs = null, Color[] clr = null)
            : base(RWType.Struct)
        {
            GeometryFlags = RWGeometryFlags.HasVertexPosition | RWGeometryFlags.HasVertexNormal | RWGeometryFlags.HasVertexLighting | RWGeometryFlags.CanTriStrip | RWGeometryFlags.ModulateMatColor;

            if (uvs != null)
            {
                if (uvs.Length >= 1)
                    GeometryFlags |= RWGeometryFlags.HasTexCoord1;
                //if (uvs.Length >= 2)
                //    GeometryFlags |= RWGeometryFlags.HasTexCoord2;
            }

            if (clr != null)
                GeometryFlags |= RWGeometryFlags.HasVertexColor;

            byte[][] dummyIndices = new byte[0][];
            float[][] dummyWeights = new float[0][];

            OptimizeData(pos, nrm, ref dummyIndices, ref dummyWeights, matIds, uvs != null ? uvs[0] : null, clr);

            Triangles = Triangles.OrderBy(o => o.MatID).ToArray();

            BoundingInfo = RWBoundingInfo.Create(Positions);

            if (uvs != null)
                TexCoordCount = 1;

            NativeFlag = RWGeometryNativeFlag.Default;
            FaceCount = (uint)Triangles.Length;
            VertexCount = (uint)Positions.Length;
            MorphTargetCount = 1;
        }

        public RWGeometryStruct(ref byte[][] skinBoneIndices, ref float[][] skinBoneWeights, Vector3[] pos, Vector3[] nrm, ushort[] matIds, Vector2[][] uvs = null, Color[] clr = null)
            : base(RWType.Struct)
        {
            GeometryFlags = RWGeometryFlags.HasVertexPosition | RWGeometryFlags.HasVertexNormal | RWGeometryFlags.HasVertexLighting | RWGeometryFlags.CanTriStrip | RWGeometryFlags.ModulateMatColor;

            if (uvs != null)
            {
                if (uvs.Length >= 1)
                    GeometryFlags |= RWGeometryFlags.HasTexCoord1;
                //if (uvs.Length >= 2)
                //    GeometryFlags |= RWGeometryFlags.HasTexCoord2;
            }

            if (clr != null)
                GeometryFlags |= RWGeometryFlags.HasVertexColor;

            OptimizeData(pos, nrm, ref skinBoneIndices, ref skinBoneWeights, matIds, uvs != null ? uvs[0] : null, clr);

            Triangles = Triangles.OrderBy(o => o.MatID).ToArray();

            BoundingInfo = RWBoundingInfo.Create(Positions);

            if (uvs != null)
                TexCoordCount = 1;

            NativeFlag = RWGeometryNativeFlag.Default;
            FaceCount = (uint)Triangles.Length;
            VertexCount = (uint)Positions.Length;
            MorphTargetCount = 1;
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            writer.Write((ushort)GeometryFlags);
            writer.Write(TexCoordCount);
            writer.Write((byte)NativeFlag);
            writer.Write(FaceCount);
            writer.Write(VertexCount);
            writer.Write(MorphTargetCount);

            if ((GeometryFlags & RWGeometryFlags.HasVertexColor) == RWGeometryFlags.HasVertexColor)
                for (int i = 0; i < VertexCount; i++)
                    writer.Write(Colors[i].ToArgb());

            if ((GeometryFlags & RWGeometryFlags.HasTexCoord1) == RWGeometryFlags.HasTexCoord1 || (GeometryFlags & RWGeometryFlags.HasTexCoord2) == RWGeometryFlags.HasTexCoord2)
            {
                for (int i = 0; i < TexCoordCount; i++)
                    for (int j = 0; j < VertexCount; j++)
                    {
                        writer.Write(TexCoordSets[i][j].X); writer.Write(TexCoordSets[i][j].Y);
                    }
            }

            for (int i = 0; i < FaceCount; i++)
                Triangles[i].Write(writer);

            BoundingInfo.Write(writer);

            if ((GeometryFlags & RWGeometryFlags.HasVertexPosition) == RWGeometryFlags.HasVertexPosition)
            {
                for (int i = 0; i < VertexCount; i++)
                {
                    writer.Write(Positions[i].X); writer.Write(Positions[i].Y); writer.Write(Positions[i].Z);
                }
            }

            if ((GeometryFlags & RWGeometryFlags.HasVertexNormal) == RWGeometryFlags.HasVertexNormal)
            {
                for (int i = 0; i < VertexCount; i++)
                {
                    writer.Write(Normals[i].X); writer.Write(Normals[i].Y); writer.Write(Normals[i].Z);
                }
            }
        }
           
        private struct ProceduralVertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 texCoord;
            public Color color;
            public byte[] boneIndices;
            public float[] boneWeights;
        }

        private void OptimizeData(Vector3[] pos, Vector3[] nrm, ref byte[][] boneIndices, ref float[][] boneWeights, ushort[] matIds, Vector2[] texCoord = null, Color[] colors = null)
        {
            // Hack for meshes without skin
            if (boneIndices.Length == 0)
                boneIndices = null;

            if (boneWeights.Length == 0)
                boneWeights = null;

            // These lists will be used to store the new vertex data
            List<Vector3> newPositions = new List<Vector3>();
            List<Vector3> newNormals = new List<Vector3>();
            List<Vector2> newTexCoords = new List<Vector2>();
            List<Color> newColors = new List<Color>();
            List<byte[]> newBoneIndices = new List<byte[]>();
            List<float[]> newBoneWeights = new List<float[]>();

            // This list will be used to compare between vertices to check their equality
            List<ProceduralVertex> uniqueVertList = new List<ProceduralVertex>();

            // This list will store the new optimized triangles generated from the new vertex data
            List<RWTriangle> newTriangles = new List<RWTriangle>();

            int triIdx = -1;

            Console.WriteLine("Optimizing mesh with {0} vertices", pos.Length);

            for (int vtxIndex = 0; vtxIndex < pos.Length; vtxIndex += 3)
            {
                // Increment the triangle index each iteration for the material id list
                triIdx++;

                // Create a new empty triangle
                RWTriangle tri = new RWTriangle { MatID = matIds[triIdx] };

                // Loop 3 times, covering A, B and C of the triangle
                for (int triVtxOffset = 0; triVtxOffset < 3; triVtxOffset++)
                {
                    ProceduralVertex curVertex = new ProceduralVertex();

                    curVertex.position = pos[vtxIndex + triVtxOffset];
                    curVertex.normal = nrm[vtxIndex + triVtxOffset];

                    if (texCoord != null)
                        curVertex.texCoord = texCoord[vtxIndex + triVtxOffset];

                    if (colors != null)
                        curVertex.color = colors[vtxIndex + triVtxOffset];

                    if (boneIndices != null)
                    {
                        curVertex.boneIndices = boneIndices[vtxIndex + triVtxOffset];
                        curVertex.boneWeights = boneWeights[vtxIndex + triVtxOffset];
                    }

                    bool isMatch = false;
                    int uniqueVtxIndex = -1;
                    foreach (ProceduralVertex uniqueVertex in uniqueVertList)
                    {
                        uniqueVtxIndex++;

                        if (uniqueVertex.position == curVertex.position)
                            isMatch = true;
                        else
                            continue;

                        if (uniqueVertex.normal == curVertex.normal)
                            isMatch = true;
                        else
                        {
                            isMatch = false;
                            continue;
                        }

                        if (texCoord != null)
                        {
                            if (uniqueVertex.texCoord == curVertex.texCoord)
                                isMatch = true;
                            else
                            {
                                isMatch = false;
                                continue;
                            }
                        }

                        if (colors != null)
                        {
                            if (uniqueVertex.color == curVertex.color)
                                isMatch = true;
                            else
                            {
                                isMatch = false;
                                continue;
                            }
                        }
                       
                        if (boneIndices != null)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                if (uniqueVertex.boneIndices[i] != curVertex.boneIndices[i] ||
                                    uniqueVertex.boneWeights[i] != curVertex.boneWeights[i])
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }                     

                        if (isMatch)
                            break;
                    }

                    // We have a match
                    // The new triangle vertex index is the index of the matched vertex
                    if (isMatch)
                        tri[triVtxOffset] = (ushort)uniqueVtxIndex;
                    else
                    {
                        // We did not find a match in the unique vertex list
                        // So we add the current vertex to the list
                        uniqueVertList.Add(curVertex);

                        // Add the other attributes to their lists
                        newPositions.Add(curVertex.position);
                        newNormals.Add(curVertex.normal);

                        if (texCoord != null)
                            newTexCoords.Add(curVertex.texCoord);

                        if (colors != null)
                            newColors.Add(curVertex.color);

                        if (boneIndices != null)
                        {
                            newBoneIndices.Add(curVertex.boneIndices);
                            newBoneWeights.Add(curVertex.boneWeights);
                        }

                        // And set the triangle index to the last checked vertex + 1
                        tri[triVtxOffset] = (ushort)(uniqueVtxIndex + 1);
                    }
                }
                newTriangles.Add(tri);

                if (vtxIndex % 256 == 0)
                    Console.WriteLine("{0} out of {1} vertices optimized", vtxIndex, pos.Length);
            }

            Console.WriteLine("{0} out of {1} vertices optimized", pos.Length, pos.Length);
            Console.WriteLine("Done! Optimized vertex count = {0}, {1}% less vertices", 
				uniqueVertList.Count, ((float)(pos.Length - uniqueVertList.Count) / pos.Length) * 100);

            if (texCoord != null)
            {
                TexCoordSets = new Vector2[1][];
                TexCoordSets[0] = newTexCoords.ToArray();
            }

            if (colors != null)
                Colors = newColors.ToArray();

            if (boneIndices != null)
            {
                boneIndices = newBoneIndices.ToArray();
                boneWeights = newBoneWeights.ToArray();
            }

            Triangles = newTriangles.ToArray();
            Positions = newPositions.ToArray();
            Normals = newNormals.ToArray();
        }
        
    }
}