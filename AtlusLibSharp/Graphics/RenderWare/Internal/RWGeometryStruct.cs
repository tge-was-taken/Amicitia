using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using AtlusLibSharp.Utilities;

    internal class RWGeometryStruct : RWNode
    {
        private RWGeometryFlags _geoFlags;
        private RWGeometryNativeFlag _nativeFlag;
        private Color[] _clrArray;
        private Vector2[][] _texCoordSets;
        private RWTriangle[] _triArray;
        private RWBoundingSphere _bSphere;
        private Vector3[] _posArray;
        private Vector3[] _nrmArray;

        public RWGeometryFlags Flags
        {
            get { return _geoFlags; }
        }

        public int TextureCoordinateChannelCount
        {
            get { return _texCoordSets == null ? 0 : _texCoordSets.Length; }
        }

        public RWGeometryNativeFlag NativeFlag
        {
            get { return _nativeFlag; }
        }

        public int TriangleCount
        {
            get { return _triArray.Length; }
        }

        public int VertexCount
        {
            get { return _posArray.Length; }
        }

        public Color[] Colors
        {
            get { return _clrArray; }
        }

        public Vector2[][] TextureCoordinateChannels
        {
            get { return _texCoordSets; }
        }

        public RWTriangle[] Triangles
        {
            get { return _triArray; }
        }

        public RWBoundingSphere BoundingSphere
        {
            get { return _bSphere; }
        }

        public Vector3[] Vertices
        {
            get { return _posArray; }
        }

        public Vector3[] Normals
        {
            get { return _nrmArray; }
        }

        public bool HasVertices
        {
            get { return _geoFlags.HasFlagUnchecked(RWGeometryFlags.HasVertices); }
        }

        public bool HasNormals
        {
            get { return _geoFlags.HasFlagUnchecked(RWGeometryFlags.HasNormals); }
        }

        public bool HasColors
        {
            get { return _geoFlags.HasFlagUnchecked(RWGeometryFlags.HasColors); }
        }

        public bool HasTexCoords
        {
            get { return _geoFlags.HasFlagUnchecked(RWGeometryFlags.HasTexCoord1); }
        }

        // TODO:
        // 1. Implement support for multiple texture coords for newly generated models
        // Priority isn't very high and requires some changes to how the optimizer works

        public RWGeometryStruct(
            Vector3[] vertexPositions, Vector3[] vertexNormals, short[] triangleMaterialIDs,
            Vector2[][] textureCoordinateSets, Color[] vertexColors,
            ref byte[][] skinBoneIndices, ref float[][] skinBoneWeights)
            : base(RWType.Struct)
        {
            _geoFlags =
                RWGeometryFlags.HasVertices   |
                RWGeometryFlags.HasNormals     |
                RWGeometryFlags.HasVertexLighting   |
                RWGeometryFlags.CanTriStrip         |
                RWGeometryFlags.ModulateMatColor;

            if (textureCoordinateSets != null)
            {
                if (textureCoordinateSets.Length >= 1)
                    _geoFlags |= RWGeometryFlags.HasTexCoord1;
                //if (uvs.Length >= 2)
                //    GeometryFlags |= RWGeometryFlags.HasTexCoord2;
            }

            if (vertexColors != null)
                _geoFlags |= RWGeometryFlags.HasColors;

            OptimizeData(
                vertexPositions, vertexNormals, 
                textureCoordinateSets != null ? textureCoordinateSets[0] : null, vertexColors,
                triangleMaterialIDs,
                ref skinBoneIndices, ref skinBoneWeights);

            _triArray = Triangles.OrderBy(tri => tri.MatID).ToArray();

            _bSphere = RWBoundingSphere.Calculate(_posArray);

            _nativeFlag = RWGeometryNativeFlag.Default;
        }

        internal RWGeometryStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
        : base(header)
        {
            _geoFlags = (RWGeometryFlags)reader.ReadUInt16();
            byte numTexCoord = reader.ReadByte();
            _nativeFlag = (RWGeometryNativeFlag)reader.ReadByte();
            int numTris = reader.ReadInt32();
            int numVerts = reader.ReadInt32();
            int numMorphTargets = reader.ReadInt32();

            if (numMorphTargets != 1)
            {
                throw new NotImplementedException("Morph targets are not implemented");
            }

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasColors))
            {
                _clrArray = reader.ReadColorArray(numVerts);
            }

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasTexCoord1) ||
               (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasTexCoord2)))
            {
                _texCoordSets = new Vector2[numTexCoord][];

                for (int i = 0; i < numTexCoord; i++)
                {
                    _texCoordSets[i] = reader.ReadVector2Array(numVerts);
                }
            }

            _triArray = new RWTriangle[numTris];
            for (int i = 0; i < numTris; i++)
            {
                _triArray[i] = new RWTriangle(reader);
            }

            _bSphere = new RWBoundingSphere(reader);

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasVertices))
            {
                _posArray = reader.ReadVector3Array(numVerts);
            }

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasNormals))
            {
                _nrmArray = reader.ReadVector3Array(numVerts);
            }
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write((ushort)_geoFlags);
            writer.Write((byte)TextureCoordinateChannelCount);
            writer.Write((byte)_nativeFlag);
            writer.Write(TriangleCount);
            writer.Write(VertexCount);
            writer.Write(1); // morph target count

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasColors))
            {
                writer.Write(_clrArray);
            }

            if (_geoFlags.HasFlag(RWGeometryFlags.HasTexCoord1) || 
               (_geoFlags.HasFlag(RWGeometryFlags.HasTexCoord2)))
            {
                for (int i = 0; i < TextureCoordinateChannelCount; i++)
                {
                    writer.Write(_texCoordSets[i]);
                }
            }

            for (int i = 0; i < TriangleCount; i++)
            {
                _triArray[i].InternalWrite(writer);
            }

            _bSphere.InternalWrite(writer);

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasVertices))
            {
                writer.Write(_posArray);
            }

            if (_geoFlags.HasFlagUnchecked(RWGeometryFlags.HasNormals))
            {
                writer.Write(_nrmArray);
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

        private void OptimizeData(
            Vector3[] vertices, Vector3[] normals, Vector2[] texCoord, Color[] colors,
            short[] matIds,
            ref byte[][] boneIndices, ref float[][] boneWeights)
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

            Console.WriteLine("Optimizing mesh with {0} vertices", vertices.Length);

            for (int vtxIndex = 0; vtxIndex < vertices.Length; vtxIndex += 3)
            {
                // Increment the triangle index each iteration for the material id list
                triIdx++;

                // Create a new empty triangle
                RWTriangle tri = new RWTriangle { MatID = matIds[triIdx] };

                // Loop 3 times, covering A, B and C of the triangle
                for (int triVtxOffset = 0; triVtxOffset < 3; triVtxOffset++)
                {
                    ProceduralVertex curVertex = new ProceduralVertex();

                    curVertex.position = vertices[vtxIndex + triVtxOffset];
                    curVertex.normal = normals[vtxIndex + triVtxOffset];

                    if (texCoord != null)
                        curVertex.texCoord = texCoord[vtxIndex + triVtxOffset];

                    if (colors != null)
                        curVertex.color = colors[vtxIndex + triVtxOffset];

                    if (boneIndices != null)
                    {
                        curVertex.boneIndices = boneIndices[vtxIndex + triVtxOffset].ToArray();
                        curVertex.boneWeights = boneWeights[vtxIndex + triVtxOffset].ToArray();
                    }

                    bool isMatch = false;
                    int uniqueVtxIndex = -1;
                    foreach (ProceduralVertex uniqueVertex in uniqueVertList)
                    {
                        uniqueVtxIndex++;

                        if (uniqueVertex.position == curVertex.position)
                        {
                            isMatch = true;
                        }
                        else
                        {
                            isMatch = true; // in case everything breaks, remove this line
                            continue;
                        }

                        if (uniqueVertex.normal == curVertex.normal)
                        {
                            isMatch = true;
                        }
                        else
                        {
                            isMatch = false;
                            continue;
                        }

                        if (texCoord != null)
                        {
                            if (uniqueVertex.texCoord == curVertex.texCoord)
                            {
                                isMatch = true;
                            }
                            else
                            {
                                isMatch = false;
                                continue;
                            }
                        }

                        if (colors != null)
                        {
                            if (uniqueVertex.color == curVertex.color)
                            {
                                isMatch = true;
                            }
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
                    {
                        tri[triVtxOffset] = (ushort)uniqueVtxIndex;
                    }
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
                    Console.WriteLine("{0} out of {1} vertices optimized", vtxIndex, vertices.Length);
            }

            Console.WriteLine("{0} out of {1} vertices optimized", vertices.Length, vertices.Length);
            Console.WriteLine("Done! Optimized vertex count = {0}, {1}% less vertices", 
				uniqueVertList.Count, ((float)(vertices.Length - uniqueVertList.Count) / vertices.Length) * 100);

            if (texCoord != null)
            {
                _texCoordSets = new Vector2[1][];
                _texCoordSets[0] = newTexCoords.ToArray();
            }

            if (colors != null)
                _clrArray = newColors.ToArray();

            if (boneIndices != null)
            {
                boneIndices = newBoneIndices.ToArray();
                boneWeights = newBoneWeights.ToArray();
            }

            _triArray = newTriangles.ToArray();
            _posArray = newPositions.ToArray();
            _nrmArray = newNormals.ToArray();
        }
        
    }
}