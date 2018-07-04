namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Numerics;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Utilities;

    internal class RwGeometryStructNode : RwNode
    {
        private const int SUPPORTED_MORPH_COUNT = 1;

        private Color[] mClrArray;
        private Vector2[][] mTexCoordSets;
        private Vector3[] mPosArray;
        private Vector3[] mNrmArray;

        public RwGeometryFlags Flags { get; set; }

        public int TextureCoordinateChannelCount => mTexCoordSets?.Length ?? 0;

        public RwGeometryNativeFlag NativeFlag { get; private set; }

        public int TriangleCount => Triangles.Length;

        public int VertexCount => mPosArray.Length;

        public Color[] Colors
        {
            get => mClrArray;
            set
            {
                mClrArray = value;
                Flags &= ~RwGeometryFlags.HasColors;

                if (value != null)
                    Flags |= RwGeometryFlags.HasColors;
            }
        }

        public Vector2[][] TextureCoordinateChannels
        {
            get => mTexCoordSets;
            set
            {
                mTexCoordSets = value;
                Flags &= RwGeometryFlags.HasTexCoord1;
                Flags &= RwGeometryFlags.HasTexCoord2;

                if ( mTexCoordSets != null && mTexCoordSets.Length > 0 )
                {
                    if ( mTexCoordSets.Length >= 1 )
                        Flags |= RwGeometryFlags.HasTexCoord1;

                    if ( mTexCoordSets.Length >= 2 )
                        Flags |= RwGeometryFlags.HasTexCoord2;
                }
            }
        }

        public RwTriangle[] Triangles { get; set; }

        public RwBoundingSphere BoundingSphere { get; set; }

        public Vector3[] Vertices
        {
            get => mPosArray;
            set
            {
                mPosArray = value;
                Flags &= ~RwGeometryFlags.HasVertices;

                if ( value != null )
                    Flags |= RwGeometryFlags.HasVertices;
            }
        }

        public Vector3[] Normals
        {
            get => mNrmArray;
            set
            {
                mNrmArray = value;
                Flags &= ~RwGeometryFlags.HasNormals;

                if ( value != null )
                    Flags |= RwGeometryFlags.HasNormals;
            }
        }

        public bool HasVertices => Flags.HasFlagUnchecked(RwGeometryFlags.HasVertices);

        public bool HasNormals => Flags.HasFlagUnchecked(RwGeometryFlags.HasNormals);

        public bool HasColors => Flags.HasFlagUnchecked(RwGeometryFlags.HasColors);

        public bool HasTexCoords => Flags.HasFlagUnchecked(RwGeometryFlags.HasTexCoord1);

        // TODO:
        // 1. Implement support for multiple texture coords for newly generated models
        // Priority isn't very high and requires some changes to how the optimizer works

        public RwGeometryStructNode(
            Vector3[] vertexPositions, Vector3[] vertexNormals, short[] triangleMaterialIDs,
            Vector2[][] textureCoordinateSets, Color[] vertexColors,
            ref byte[][] skinBoneIndices, ref float[][] skinBoneWeights)
                : base(RwNodeId.RwStructNode)
        {
            Initialize(vertexPositions, vertexNormals, triangleMaterialIDs, textureCoordinateSets, vertexColors,
                ref skinBoneIndices, ref skinBoneWeights);
        }

        private static Vector3[] ExtractVertices(Assimp.Mesh mesh, List<Assimp.Vector3D> buffer)
        {
            var vertices = new Vector3[mesh.Faces.Count * 3];
            for ( int i = 0; i < mesh.Faces.Count; i++ )
            {
                for ( int j = 0; j < 3; j++ )
                {
                    var assimpVector3 = buffer[mesh.Faces[i].Indices[j]];
                    vertices[( i * 3 ) + j] = new Vector3( assimpVector3.X, assimpVector3.Y, assimpVector3.Z );
                }
            }

            return vertices;
        }

        private static Vector2[][] ExtractTexCoordChannels(Assimp.Mesh mesh)
        {
            var texCoordChannels = new Vector2[mesh.TextureCoordinateChannelCount][];
            for ( int channelIdx = 0; channelIdx < mesh.TextureCoordinateChannelCount; channelIdx++ )
            {
                texCoordChannels[channelIdx] = new Vector2[mesh.Faces.Count * 3];

                for ( int faceIdx = 0; faceIdx < mesh.Faces.Count; faceIdx++ )
                {
                    for ( int vertIdx = 0; vertIdx < 3; vertIdx++ )
                    {
                        var assimpVector3 = mesh.TextureCoordinateChannels[channelIdx][mesh.Faces[faceIdx].Indices[vertIdx]];
                        texCoordChannels[channelIdx][( faceIdx * 3 ) + vertIdx] = new Vector2( assimpVector3.X, assimpVector3.Y );
                    }
                }
            }

            return texCoordChannels;
        }

        private static Color[] ExtractAllColors(Assimp.Mesh mesh)
        {
            var vertexColors = new Color[mesh.Faces.Count * 3];
            for ( int i = 0; i < mesh.Faces.Count; i++ )
            {
                for ( int j = 0; j < 3; j++ )
                {
                    var inColor = mesh.VertexColorChannels[0][mesh.Faces[i].Indices[j]];
                    vertexColors[( i * 3 ) + j] = Color.FromArgb( (byte)( inColor.A * 255f ), (byte)( inColor.R * 255f ), (byte)( inColor.G * 255f ), (byte)( inColor.B * 255f ) );
                }
            }

            return vertexColors;
        }

        private static void ExtractVertexWeightData(Assimp.Mesh mesh, RwFrameListNode frameList, out byte[][] skinBoneIndices, out float[][] skinBoneWeights)
        {
            skinBoneIndices = new byte[mesh.Faces.Count * 3][];
            for ( int i = 0; i < skinBoneIndices.Length; i++ )
                skinBoneIndices[i] = new byte[4];

            skinBoneWeights = new float[mesh.Faces.Count * 3][];
            for ( int i = 0; i < skinBoneWeights.Length; i++ )
                skinBoneWeights[i] = new float[4];

            foreach (var bone in mesh.Bones)
            {
                int boneIndex = frameList.GetHierarchyIndexByName(bone.Name);

                foreach (var vertexWeight in bone.VertexWeights)
                {
                    int vertexId = -1;

                    for ( int k = 0; k < mesh.Faces.Count; k++ )
                    {
                        for ( int l = 0; l < mesh.Faces[k].Indices.Count; l++ )
                        {
                            if ( mesh.Faces[k].Indices[l] == vertexWeight.VertexID )
                            {
                                vertexId = ( k * 3 ) + l;
                                break;
                            }
                        }

                        if ( vertexId != -1 )
                            break;
                    }

                    for ( int k = 0; k < 4; k++ )
                    {
                        if ( skinBoneWeights[vertexId][k] == 0.0f )
                        {
                            skinBoneIndices[vertexId][k] = (byte)boneIndex;
                            skinBoneWeights[vertexId][k] = vertexWeight.Weight;
                            break;
                        }
                    }
                }
            }
        }

        private static void PretransformVertices( Vector3[] vertices, RwFrame frame, bool isNormals )
        {
            Matrix4x4.Invert( frame.WorldTransform, out Matrix4x4 matrix );

            for (int i = 0; i < vertices.Length; i++)
            {
                if (!isNormals)
                    vertices[i] = Vector3.Transform(vertices[i], matrix);
                else
                    vertices[i] = Vector3.TransformNormal(vertices[i], matrix);
            }
        }

        public RwGeometryStructNode(RwNode parent, Assimp.Mesh mesh, RwFrameListNode frameList, bool forceSingleWeight, out byte[][] skinBoneIndices, out float[][] skinBoneWeights, out bool singleWeight)
            : base(RwNodeId.RwStructNode, parent)
        {
            singleWeight = mesh.BoneCount == 0 || mesh.BoneCount == 1 || forceSingleWeight;
            RwFrame singleFrame = null;
            if ( singleWeight )
            {
                if ( mesh.BoneCount != 0 )
                {
                    singleFrame = frameList[frameList.GetFrameIndexByName(mesh.Bones[0].Name)];
                }
                else
                {
                    singleFrame = frameList.FirstOrDefault(x => x.HasHAnimExtension) ?? frameList[0];
                }
            }

            if ( mesh.HasBones && !singleWeight )
            {
                ExtractVertexWeightData( mesh, frameList, out skinBoneIndices, out skinBoneWeights );
            }
            else
            {
                skinBoneIndices = null;
                skinBoneWeights = null;
            }

            var vertexPositions = ExtractVertices( mesh, mesh.Vertices );
            if ( singleWeight )
                PretransformVertices( vertexPositions, singleFrame, false );

            Vector3[] vertexNormals = null;
            if ( mesh.HasNormals )
            {
                vertexNormals = ExtractVertices( mesh, mesh.Normals );
                if ( singleWeight )
                    PretransformVertices( vertexNormals, singleFrame, true );
            }

            Vector2[][] vertexTextureCoordinateSets = null;
            if ( mesh.TextureCoordinateChannelCount > 0 )
                vertexTextureCoordinateSets = ExtractTexCoordChannels( mesh );

            Color[] vertexColors = null;
            if ( mesh.VertexColorChannelCount > 0 )
                vertexColors = ExtractAllColors( mesh );

            Initialize( vertexPositions, vertexNormals, null, vertexTextureCoordinateSets, vertexColors,
                ref skinBoneIndices, ref skinBoneWeights );
        }

        internal RwGeometryStructNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            Flags = (RwGeometryFlags)reader.ReadUInt16();
            byte numTexCoord = reader.ReadByte();
            NativeFlag = (RwGeometryNativeFlag)reader.ReadByte();
            int numTris = reader.ReadInt32();
            int numVerts = reader.ReadInt32();
            int numMorphTargets = reader.ReadInt32();

            if (Flags.HasFlagUnchecked(RwGeometryFlags.HasColors))
            {
                mClrArray = reader.ReadColorArray(numVerts);
            }

            if (Flags.HasFlagUnchecked(RwGeometryFlags.HasTexCoord1) ||
               (Flags.HasFlagUnchecked(RwGeometryFlags.HasTexCoord2)))
            {
                mTexCoordSets = new Vector2[numTexCoord][];

                for (int i = 0; i < numTexCoord; i++)
                {
                    mTexCoordSets[i] = reader.ReadVector2Array(numVerts);
                }
            }

            Triangles = new RwTriangle[numTris];
            for (int i = 0; i < numTris; i++)
            {
                Triangles[i] = new RwTriangle(reader);
            }

            for ( int i = 0; i < numMorphTargets; i++ )
            {
                BoundingSphere = new RwBoundingSphere( reader );

                if ( Flags.HasFlagUnchecked( RwGeometryFlags.HasVertices ) )
                {
                    var positions = reader.ReadVector3Array( numVerts );
                    if ( i == 0 )
                        mPosArray = positions;
                }

                if ( Flags.HasFlagUnchecked( RwGeometryFlags.HasNormals ) )
                {
                    var normals = reader.ReadVector3Array( numVerts );
                    if ( i == 0 )
                        mNrmArray = normals;
                }
            }
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            writer.Write((ushort)Flags);
            writer.Write((byte)TextureCoordinateChannelCount);
            writer.Write((byte)NativeFlag);
            writer.Write(TriangleCount);
            writer.Write(VertexCount);
            writer.Write(SUPPORTED_MORPH_COUNT);

            if (Flags.HasFlagUnchecked(RwGeometryFlags.HasColors))
            {
                writer.Write(mClrArray);
            }

            if (Flags.HasFlag(RwGeometryFlags.HasTexCoord1) || 
               (Flags.HasFlag(RwGeometryFlags.HasTexCoord2)))
            {
                for (int i = 0; i < TextureCoordinateChannelCount; i++)
                {
                    writer.Write(mTexCoordSets[i]);
                }
            }

            for (int i = 0; i < TriangleCount; i++)
            {
                Triangles[i].InternalWrite(writer);
            }

            BoundingSphere.Write(writer);

            if (Flags.HasFlagUnchecked(RwGeometryFlags.HasVertices))
            {
                writer.Write(mPosArray);
            }

            if (Flags.HasFlagUnchecked(RwGeometryFlags.HasNormals))
            {
                writer.Write(mNrmArray);
            }
        }
           
        private struct ProceduralVertex
        {
            public Vector3 position;
            public Vector3 normal;
            public Vector2 TexCoord;
            public Color color;
            public byte[] boneIndices;
            public float[] boneWeights;
        }

        private void Initialize(
            IReadOnlyList<Vector3> vertexPositions, IReadOnlyList<Vector3> vertexNormals, IReadOnlyList<short> triangleMaterialIDs,
            IReadOnlyList<Vector2[]> textureCoordinateSets, IReadOnlyList<Color> vertexColors,
            ref byte[][] skinBoneIndices, ref float[][] skinBoneWeights)
        {
            Flags =
                RwGeometryFlags.HasVertices |
                RwGeometryFlags.HasVertexLighting;

            if (vertexNormals != null)
            {
                Flags |= RwGeometryFlags.HasNormals;
            }

            if (textureCoordinateSets != null)
            {
                if (textureCoordinateSets.Count >= 1)
                    Flags |= RwGeometryFlags.HasTexCoord1;

                //if (textureCoordinateSets.Length >= 2)
                    //mGeoFlags |= RwGeometryFlags.HasTexCoord2;
            }

            if (vertexColors != null)
                Flags |= RwGeometryFlags.HasColors | RwGeometryFlags.ModulateMatColor;

            OptimizeData(
                vertexPositions, vertexNormals,
                textureCoordinateSets?[0], vertexColors,
                triangleMaterialIDs,
                ref skinBoneIndices, ref skinBoneWeights);

            Triangles = Triangles.OrderBy(tri => tri.MatId).ToArray();

            BoundingSphere = RwBoundingSphere.Calculate(mPosArray);

            NativeFlag = RwGeometryNativeFlag.Default;
        }

        private void OptimizeData(
            IReadOnlyList<Vector3> vertices, IReadOnlyList<Vector3> normals, IReadOnlyList<Vector2> texCoord, IReadOnlyList<Color> colors,
            IReadOnlyList<short> matIds,
            ref byte[][] boneIndices, ref float[][] boneWeights)
        {
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
            List<RwTriangle> newTriangles = new List<RwTriangle>();

            int triIdx = -1;

            Console.WriteLine("Optimizing mesh with {0} vertices", vertices.Count);

            for (int vtxIndex = 0; vtxIndex < vertices.Count; vtxIndex += 3)
            {
                // Increment the triangle index each iteration for the material id list
                triIdx++;

                // Create a new empty triangle
                RwTriangle tri = new RwTriangle { MatId = matIds?[triIdx] ?? 0 };

                // Loop 3 times, covering A, B and C of the triangle
                for (int triVtxOffset = 0; triVtxOffset < 3; triVtxOffset++)
                {
                    ProceduralVertex curVertex = new ProceduralVertex
                    {
                        position = vertices[vtxIndex + triVtxOffset],
                        normal = normals[vtxIndex + triVtxOffset]
                    };


                    if (texCoord != null)
                        curVertex.TexCoord = texCoord[vtxIndex + triVtxOffset];

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
                            isMatch = false; // in case everything breaks, remove this line
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
                            if (uniqueVertex.TexCoord == curVertex.TexCoord)
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
                            newTexCoords.Add(curVertex.TexCoord);

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
                    Console.WriteLine("{0} out of {1} vertices optimized", vtxIndex, vertices.Count);
            }

            Console.WriteLine("{0} out of {1} vertices optimized", vertices.Count, vertices.Count);
            Console.WriteLine("Done! Optimized vertex count = {0}, {1}% less vertices", 
				uniqueVertList.Count, ((float)(vertices.Count - uniqueVertList.Count) / vertices.Count) * 100);

            if (texCoord != null)
            {
                mTexCoordSets = new Vector2[1][];
                mTexCoordSets[0] = newTexCoords.ToArray();
            }

            if (colors != null)
                mClrArray = newColors.ToArray();

            if (boneIndices != null)
            {
                boneIndices = newBoneIndices.ToArray();
                boneWeights = newBoneWeights.ToArray();
            }

            Triangles = newTriangles.ToArray();
            mPosArray = newPositions.ToArray();
            mNrmArray = newNormals.ToArray();
        }
        
    }
}