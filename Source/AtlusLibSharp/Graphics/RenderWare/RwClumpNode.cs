using System;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using Utilities;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Numerics;

    /// <summary>
    /// Encapsulates a RenderWare model clumpNode containing meshes, draw calls, lights and cameras.
    /// </summary>
    public class RwClumpNode : RwNode
    {
        private RwClumpStructNode mStructNode;
        private readonly RwExtensionNode mExtensionNodeNode;

        #region Properties

        /// <summary>
        /// Gets the listNode of draw calls in the clumpNode.
        /// </summary>
        public List<RwAtomicNode> Atomics { get; }

        /// <summary>
        /// Gets the number of draw calls in the clumpNode.
        /// </summary>
        public int AtomicCount
        {
            get { return Atomics.Count; }
        }

        /// <summary>
        /// Gets if the clumpNode has any draw calls.
        /// </summary>
        public bool HasAtomics
        {
            get { return AtomicCount > 0; }
        }

        /// <summary>
        /// Gets the number of geometries in the clumpNode.
        /// </summary>
        public int GeometryCount
        {
            get { return GeometryList.Count; }
        }

        /// <summary>
        /// Gets if the clumpNode has any geometries.
        /// </summary>
        public bool HasGeometries
        {
            get { return GeometryCount > 0; }
        }

        /**************************************/
        /* RWSceneStruct forwarded properties */
        /**************************************/

        /// <summary>
        /// Gets the number of lights in the clumpNode.
        /// </summary>
        public int LightCount
        {
            get { return mStructNode.LightCount; }
        }

        /// <summary>
        /// Gets the number of cameras in the clumpNode.
        /// </summary>
        public int CameraCount
        {
            get { return mStructNode.CameraCount; }
        }

        /// <summary>
        /// Gets the listNode of clumpNode nodes in the clumpNode.
        /// </summary>
        public RwFrameListNode FrameList { get; }

        /// <summary>
        /// Gets the listNode of geometries in this clumpNode.
        /// </summary>
        public RwGeometryListNode GeometryList { get; }

        /// <summary>
        /// Gets the extension nodes for this clumpNode.
        /// </summary>
        public List<RwNode> Extensions
        {
            get { return mExtensionNodeNode.Children; }
        }

        #endregion

        private static void WalkChildren(RwFrame rwNode, Assimp.Node parent)
        {
            foreach (RwFrame child in rwNode.Children)
            {
                string name = child.HAnimFrameExtensionNode.NameId.ToString();
                Assimp.Node aiNode = new Assimp.Node(name, parent);
            }
        }

        public static Assimp.Scene ToAssimpScene(RwClumpNode clumpNode)
        {
            // Scene
            var aiScene = new Assimp.Scene();

            // RootNode
            var rootFrame = clumpNode.FrameList[0];
            var aiRootNode = new Assimp.Node( "SceneRoot", null );
            aiRootNode.Transform = new Assimp.Matrix4x4( rootFrame.Transform.M11, rootFrame.Transform.M21, rootFrame.Transform.M31, rootFrame.Transform.M41,
                                                         rootFrame.Transform.M12, rootFrame.Transform.M22, rootFrame.Transform.M32, rootFrame.Transform.M42,
                                                         rootFrame.Transform.M13, rootFrame.Transform.M23, rootFrame.Transform.M33, rootFrame.Transform.M43,
                                                         rootFrame.Transform.M14, rootFrame.Transform.M24, rootFrame.Transform.M34, rootFrame.Transform.M44 );

            aiScene.RootNode = aiRootNode;

            for ( int i = 1; i < clumpNode.FrameList.Count - 1; i++ )
            {
                var frame = clumpNode.FrameList[i];
                var frameName = frame.HAnimFrameExtensionNode.NameId.ToString();

                Assimp.Node aiParentNode = null;
                if (frame.Parent != null)
                {
                    string parentName = "SceneRoot";
                    if (frame.Parent.HasHAnimExtension)
                    {
                        parentName = frame.Parent.HAnimFrameExtensionNode.NameId.ToString();
                    }

                    aiParentNode = aiRootNode.FindNode( parentName );
                }

                var aiNode = new Assimp.Node( frameName, aiParentNode );
                aiNode.Transform = new Assimp.Matrix4x4( frame.Transform.M11, frame.Transform.M21, frame.Transform.M31, frame.Transform.M41,
                                                         frame.Transform.M12, frame.Transform.M22, frame.Transform.M32, frame.Transform.M42,
                                                         frame.Transform.M13, frame.Transform.M23, frame.Transform.M33, frame.Transform.M43,
                                                         frame.Transform.M14, frame.Transform.M24, frame.Transform.M34, frame.Transform.M44 );
                aiParentNode.Children.Add( aiNode );
            }

            // Meshes, Materials
            for ( int atomicIndex = 0; atomicIndex < clumpNode.Atomics.Count; atomicIndex++ )
            {
                var atomic   = clumpNode.Atomics[atomicIndex];
                var geometry = clumpNode.GeometryList[atomic.GeometryIndex];
                var frame    = clumpNode.FrameList[atomic.FrameIndex];

                var aiNodeName = $"Atomic{atomicIndex}";
                var aiNode = new Assimp.Node( aiNodeName, aiScene.RootNode );
                var frameWorldTransform = frame.WorldTransform;
                aiNode.Transform = new Assimp.Matrix4x4( frameWorldTransform.M11, frameWorldTransform.M21, frameWorldTransform.M31, frameWorldTransform.M41,
                                                         frameWorldTransform.M12, frameWorldTransform.M22, frameWorldTransform.M32, frameWorldTransform.M42,
                                                         frameWorldTransform.M13, frameWorldTransform.M23, frameWorldTransform.M33, frameWorldTransform.M43,
                                                         frameWorldTransform.M14, frameWorldTransform.M24, frameWorldTransform.M34, frameWorldTransform.M44 );
                aiScene.RootNode.Children.Add( aiNode );

                bool hasVertexWeights = geometry.SkinNode != null;

                for ( int meshIndex = 0; meshIndex < geometry.MeshListNode.MaterialMeshes.Length; meshIndex++ )
                {
                    var mesh = geometry.MeshListNode.MaterialMeshes[meshIndex];
                    var aiMesh = new Assimp.Mesh( $"Atomic{atomicIndex}_Geometry{atomic.GeometryIndex}_Mesh{meshIndex}", Assimp.PrimitiveType.Triangle );

                    // get triangle list indices
                    int[] indices;

                    if ( geometry.MeshListNode.PrimitiveType == RwPrimitiveType.TriangleList )
                    {
                        indices = mesh.Indices;
                    }
                    else
                    {
                        indices = MeshUtilities.ToTriangleList( mesh.Indices, false );
                    }

                    // Faces
                    for ( int i = 0; i < indices.Length; i += 3 )
                    {
                        var faceIndices = new[] { i, i + 1, i + 2 };
                        var aiFace = new Assimp.Face( faceIndices );
                        aiMesh.Faces.Add( aiFace );
                    }

                    // TextureCoordinateChannels, VertexColorChannels, Vertices, MaterialIndex, Normals
                    for ( int triIdx = 0; triIdx < indices.Length; triIdx += 3 )
                    {
                        for ( int triVertIdx = 0; triVertIdx < 3; triVertIdx++ )
                        {
                            int vertexIndex = indices[triIdx + triVertIdx];

                            // TextureCoordinateChannels
                            if ( geometry.HasTextureCoordinates )
                            {
                                for ( int channelIdx = 0; channelIdx < geometry.TextureCoordinateChannelCount; channelIdx++ )
                                {
                                    var textureCoordinate = geometry.TextureCoordinateChannels[channelIdx][vertexIndex];
                                    var aiTextureCoordinate = new Assimp.Vector3D( textureCoordinate.X, textureCoordinate.Y, 0f );
                                    aiMesh.TextureCoordinateChannels[channelIdx].Add( aiTextureCoordinate );
                                }
                            }

                            // VertexColorChannels
                            if ( geometry.HasColors )
                            {
                                var color = geometry.Colors[vertexIndex];
                                var aiColor = new Assimp.Color4D( color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f );
                                aiMesh.VertexColorChannels[0].Add( aiColor );
                            }

                            // Vertices
                            if ( geometry.HasVertices )
                            {
                                var vertex = geometry.Vertices[vertexIndex];
                                var aiVertex = new Assimp.Vector3D( vertex.X, vertex.Y, vertex.Z );
                                aiMesh.Vertices.Add( aiVertex );
                            }

                            // Normals
                            if ( geometry.HasNormals )
                            {
                                var normal = geometry.Normals[vertexIndex];
                                var aiNormal = new Assimp.Vector3D( normal.X, normal.Y, normal.Z );
                                aiMesh.Normals.Add( aiNormal );
                            }
                        }
                    }

                    // Bones
                    if (hasVertexWeights)
                    {
                        var skinNode = geometry.SkinNode;
                        var aiBoneMap = new Dictionary<int, Assimp.Bone>();

                        for ( int i = 0; i < indices.Length; i++ )
                        {
                            var vertexIndex = indices[i];
                            int realVertexIndex = i;

                            for ( int j = 0; j < 4; j++ )
                            {
                                var boneIndex = skinNode.VertexBoneIndices[vertexIndex][j];
                                var boneWeight = skinNode.VertexBoneWeights[vertexIndex][j];

                                if ( boneWeight == 0.0f )
                                    continue;

                                if (!aiBoneMap.Keys.Contains( boneIndex ) )
                                {
                                    var aiBone = new Assimp.Bone();
                                    var boneFrame = clumpNode.FrameList.GetFrameByHierarchyIndex( boneIndex );

                                    aiBone.Name = boneFrame.HasHAnimExtension ? boneFrame.HAnimFrameExtensionNode.NameId.ToString() : "SceneRoot";
                                    aiBone.VertexWeights.Add( new Assimp.VertexWeight( realVertexIndex, boneWeight ) );

                                    Matrix4x4.Invert( frame.WorldTransform, out Matrix4x4 invertedFrameWorldTransform );
                                    Matrix4x4.Invert( boneFrame.WorldTransform * invertedFrameWorldTransform, out Matrix4x4 offsetMatrix );
                                    aiBone.OffsetMatrix = new Assimp.Matrix4x4( offsetMatrix.M11, offsetMatrix.M21, offsetMatrix.M31, offsetMatrix.M41,
                                                                                offsetMatrix.M12, offsetMatrix.M22, offsetMatrix.M32, offsetMatrix.M42,
                                                                                offsetMatrix.M13, offsetMatrix.M23, offsetMatrix.M33, offsetMatrix.M43,
                                                                                offsetMatrix.M14, offsetMatrix.M24, offsetMatrix.M34, offsetMatrix.M44 );
                                    aiBoneMap[boneIndex] = aiBone;
                                }

                                if ( !aiBoneMap[boneIndex].VertexWeights.Any( x => x.VertexID == realVertexIndex ) )
                                    aiBoneMap[boneIndex].VertexWeights.Add( new Assimp.VertexWeight( realVertexIndex, boneWeight ) );
                            }
                        }

                        aiMesh.Bones.AddRange( aiBoneMap.Values );
                    }
                    else
                    {
                        var aiBone = new Assimp.Bone();

                        // Name
                        aiBone.Name = frame.HasHAnimExtension ? frame.HAnimFrameExtensionNode.NameId.ToString() : "SceneRoot";

                        // VertexWeights
                        for ( int i = 0; i < aiMesh.Vertices.Count; i++ )
                        {
                            var aiVertexWeight = new Assimp.VertexWeight( i, 1f );
                            aiBone.VertexWeights.Add( aiVertexWeight );
                        }

                        // OffsetMatrix
                        /*
                        Matrix4x4.Invert( frame.WorldTransform, out Matrix4x4 offsetMatrix );
                        aiBone.OffsetMatrix = new Assimp.Matrix4x4( offsetMatrix.M11, offsetMatrix.M21, offsetMatrix.M31, offsetMatrix.M41,
                                                                    offsetMatrix.M12, offsetMatrix.M22, offsetMatrix.M32, offsetMatrix.M42,
                                                                    offsetMatrix.M13, offsetMatrix.M23, offsetMatrix.M33, offsetMatrix.M43,
                                                                    offsetMatrix.M14, offsetMatrix.M24, offsetMatrix.M34, offsetMatrix.M44 );
                        */
                        aiBone.OffsetMatrix = Assimp.Matrix4x4.Identity;

                        aiMesh.Bones.Add( aiBone );
                    }

                    var material = geometry.Materials[mesh.MaterialIndex];
                    var aiMaterial = new Assimp.Material();

                    if ( material.IsTextured )
                    {
                        // TextureDiffuse
                        var texture = material.TextureReferenceNode;
                        aiMaterial.TextureDiffuse = new Assimp.TextureSlot(
                            texture.ReferencedTextureName, Assimp.TextureType.Diffuse, 0, Assimp.TextureMapping.FromUV, 0, 0, Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0 );
                    }

                    // Name
                    aiMaterial.Name = $"Geometry{atomic.GeometryIndex}_Material{mesh.MaterialIndex}";
                    if ( material.IsTextured )
                        aiMaterial.Name = material.TextureReferenceNode.ReferencedTextureName;

                    aiMaterial.ShadingMode = Assimp.ShadingMode.Phong;

                    // Add mesh to meshes
                    aiScene.Meshes.Add( aiMesh );

                    // Add material to materials
                    aiScene.Materials.Add( aiMaterial );

                    // MaterialIndex
                    aiMesh.MaterialIndex = aiScene.Materials.Count - 1;

                    // Add mesh index to node
                    aiNode.MeshIndices.Add( aiScene.Meshes.Count - 1 );
                }
            }

            return aiScene;
        }

        public RwClumpNode(RwNode parent)
            : base(RwNodeId.RwClumpNode, parent)
        {
            Atomics = new List<RwAtomicNode>();
            FrameList = new RwFrameListNode(this);
            GeometryList = new RwGeometryListNode(this);
            mExtensionNodeNode = new RwExtensionNode( this );
            mStructNode = new RwClumpStructNode(this);
        }

        public RwClumpNode(Stream stream, bool leaveOpen = false)
            : base(RwNodeId.RwClumpNode)
        {
            var node = (RwClumpNode) RwNode.Load(stream, leaveOpen);

            mStructNode = node.mStructNode;
            mStructNode.Parent = this;

            FrameList = node.FrameList;
            FrameList.Parent = this;

            GeometryList = node.GeometryList;
            GeometryList.Parent = this;

            Atomics = node.Atomics;
            foreach (var atomicNode in Atomics)
            {
                atomicNode.Parent = this;
            }

            mExtensionNodeNode = node.mExtensionNodeNode;
            mExtensionNodeNode.Parent = this;
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RwNodeFactory"/>.
        /// </summary>
        internal RwClumpNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwClumpStructNode>(this, reader);
            FrameList = RwNodeFactory.GetNode<RwFrameListNode>(this, reader);
            GeometryList = RwNodeFactory.GetNode<RwGeometryListNode>(this, reader);
            Atomics = new List<RwAtomicNode>(mStructNode.AtomicCount);

            for (int i = 0; i < mStructNode.AtomicCount; i++)
            {
                Atomics.Add(RwNodeFactory.GetNode<RwAtomicNode>(this, reader));
            }

            if ( RwNodeFactory.PeekNode( reader ) == RwNodeId.RwExtensionNode )
            {
                mExtensionNodeNode = RwNodeFactory.GetNode<RwExtensionNode>( this, reader );
            }
            else
            {
                mExtensionNodeNode = new RwExtensionNode( this );
            }
        }

        public void ReplaceGeometries(Assimp.Scene scene)
        {
            // replace meshes
            Matrix4x4[] inverseBoneMatrices = null;
            for (var i = 0; i < GeometryList.Count; i++)
            {
                var mesh = GeometryList[i];

                if (mesh.ExtensionNodes.Count != 0)
                {
                    var skinPlugin =
                        (RwSkinNode) mesh.ExtensionNodes.SingleOrDefault(x => x.Id == RwNodeId.RwSkinNode);

                    if (skinPlugin != null)
                    {
                        for (int j = 0; j < skinPlugin.SkinToBoneMatrices.Length; j++)
                        {
                            Matrix4x4.Invert(skinPlugin.SkinToBoneMatrices[j], out Matrix4x4 boneMatrix);
                            boneMatrix *= FrameList[Atomics.Find(x => x.GeometryIndex == i).FrameIndex].WorldTransform;
                            Matrix4x4.Invert(boneMatrix, out boneMatrix);
                            skinPlugin.SkinToBoneMatrices[j] = boneMatrix;
                        }

                        inverseBoneMatrices = skinPlugin.SkinToBoneMatrices;
                        break;
                    }
                }
            }

            /*
            var boneMatrices = mFrameNode.FrameListNode.Where(x => x.HasHAnimExtension).Select(x => x.Transform);
            var inverseBoneMatrices = boneMatrices.Select(x =>
            {
                Matrix4x4.Invert(x, out Matrix4x4 inverted);
                return inverted;
            });
            */

            GeometryList.Clear();
            Atomics.Clear();

            for (var i = 0; i < scene.Meshes.Count; i++)
            {
                var assimpMesh = scene.Meshes[i];            

                Assimp.Node RecursivelyFindRootNode(Assimp.Node node)
                {
                    if ( node.MeshIndices.Contains( i ) )
                    {
                        return node;
                    }
                    else
                    {
                        foreach ( var child in node.Children )
                        {
                            var result = RecursivelyFindRootNode( child );
                            if ( result != null )
                                return result;
                        }
                    }

                    return null;
                }

                var rootNode = RecursivelyFindRootNode( scene.RootNode );
                if (rootNode == null)
                {
                    rootNode = scene.RootNode;
                }

                if (rootNode != null)
                { 
                    var worldTransform = GetWorldTransform(rootNode);
                    var worldTransformInv = worldTransform;
                    worldTransformInv.Transpose();
                    worldTransformInv.Inverse();

                    for (int j = 0; j < assimpMesh.Vertices.Count; j++)
                    {
                        var vector = assimpMesh.Vertices[j];
                        Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4(ref vector, ref worldTransform);
                        assimpMesh.Vertices[j] = vector;
                    }

                    for (int j = 0; j < assimpMesh.Normals.Count; j++)
                    {
                        var vector = assimpMesh.Normals[j];
                        Assimp.Unmanaged.AssimpLibrary.Instance
                            .TransformVecByMatrix4(ref vector, ref worldTransformInv);
                        assimpMesh.Normals[j] = vector;
                    }
                }

                var geometryNode = new RwGeometryNode(this, assimpMesh, scene.Materials[assimpMesh.MaterialIndex], FrameList, inverseBoneMatrices, out bool singleWeight);
                GeometryList.Add( geometryNode );

                var atomicNode = new RwAtomicNode(this, 0, i, 5);
                if (singleWeight)
                {
                    if ( assimpMesh.Bones.Count != 0 )
                    {
                        atomicNode.FrameIndex = FrameList.GetFrameIndexByName( assimpMesh.Bones[0].Name );
                    }
                    else if ( rootNode != null )
                    {
                        atomicNode.FrameIndex = FrameList.GetFrameIndexByName( rootNode.Name );
                    }
                }

                Atomics.Add( atomicNode );
            }

            mStructNode = new RwClumpStructNode(this);
        }

        private Assimp.Matrix4x4 GetWorldTransform(Assimp.Node node)
        {
            var matrix = node.Transform;
            if ( node.Parent != null )
                matrix *= GetWorldTransform(node.Parent);

            return matrix;
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode = new RwClumpStructNode(this);
            mStructNode.Write(writer);
            FrameList.Write(writer);
            GeometryList.Write(writer);

            foreach (RwAtomicNode drawCall in Atomics)
            {
                drawCall.Write(writer);
            }

            if (HasAtomics)
            {
                mExtensionNodeNode.Write(writer);
            }

            var author = new RwStringNode("Model generated by Amicitia");
            author.Id = RwNodeId.RmdAuthor;
            author.Parent = this;

            author.Write(writer);
        }

        /*

        public static OBJFile ToOBJFile(RWScene clumpNode)
        {
            OBJFile obj = new OBJFile();
            obj.MaterialLibrary = new OBJMaterialLibrary();
            foreach (RWDrawCall drawCall in clumpNode.DrawCallList)
            {
                RWFrame frame = clumpNode.FrameListNode.StructNode.FrameListNode[drawCall.StructNode.frameIndex];
                RWGeometry geo = clumpNode.GeometryListNode.GeometryListNode[drawCall.StructNode.geometryIndex];

                Vector3[] transPositions = new Vector3[geo.StructNode.Positions.Length];
                for (int i = 0; i < geo.StructNode.Positions.Length; i++)
                    transPositions[i] = Vector3.Transform(geo.StructNode.Positions[i], frame.WorldMatrix);

                Vector3[] transNormals = new Vector3[geo.StructNode.Normals.Length];
                for (int i = 0; i < geo.StructNode.Normals.Length; i++)
                    transNormals[i] = Vector3.TransformNormal(geo.StructNode.Normals[i], frame.WorldMatrix);

                Vector2[] uvs = geo.StructNode.TexCoordSets.Length > 0 ? geo.StructNode.TexCoordSets[0] : null;
                OBJFace[] faces = new OBJFace[geo.StructNode.Triangles.Length];
                for (int i = 0; i < geo.StructNode.Triangles.Length; i++)
                {
                    List<int> indices = new List<int> { geo.StructNode.Triangles[i].A + 1, geo.StructNode.Triangles[i].B + 1, geo.StructNode.Triangles[i].C + 1 };
                    faces[i] = new OBJFace("geometry_" + drawCall.StructNode.geometryIndex + "_material_" + geo.StructNode.Triangles[i].MatID,
                        indices, // Position Indices
                        uvs != null ? indices : null, // UV Indices
                        indices); // Normal Indices
                }
                obj.Groups.Add(new OBJGroup("drawCall_" + clumpNode.DrawCallList.IndexOf(drawCall), transPositions, uvs, transNormals, faces));
                int idx = -1;
                foreach (RWMaterial material in geo.MaterialListNode.Materials)
                {
                    idx++;
                    OBJMaterial objMat = new OBJMaterial("geometry_" + drawCall.StructNode.geometryIndex + "_material_" + idx);
                    objMat.SetMap(OBJMapType.Diffuse, material.TextureReferenceNode != null ? material.TextureReferenceNode.Name : "");
                    obj.MaterialLibrary.Materials.Add(objMat);
                }
            }
            return obj;
        }

        public static SMDFile ToSMDFile(RWScene clumpNode)
        {
            SMDFile smd = new SMDFile();
            SMDFrame frame = new SMDFrame(0);
            foreach (RWFrame rwFrame in clumpNode.FrameListNode.StructNode.FrameListNode)
            {
                RWHierarchyAnimPlugin plugin = null;

                if (clumpNode.FrameListNode.Extensions[rwFrame.Index].Children != null)
                    plugin = clumpNode.FrameListNode.Extensions[rwFrame.Index].Children[0] as RWHierarchyAnimPlugin;

                string name = "Root";

                if (plugin != null)
                {
                    name = plugin.FrameNameID.ToString();
                    //name = clumpNode.FrameListNode.GetHierarchyIndexByNameID((uint)plugin.FrameNameID) + "_" + Array.IndexOf(clumpNode.FrameListNode.StructNode.FrameListNode, rwFrame);
                }

                smd.Nodes.Add(new SMDNode(smd.Nodes, rwFrame.Index, name, rwFrame.ParentIndex));
                frame.BoneFrames.Add(new SMDBoneFrame(null, rwFrame.Index, rwFrame.LocalMatrix.ExtractTranslation(), SMDFile.MatrixToEuler(rwFrame.LocalMatrix.Inverted())));
            }
            smd.FrameListNode.Add(frame);

            foreach (RWDrawCall drawCall in clumpNode.DrawCallList)
            {
                RWFrame rwFrame = clumpNode.FrameListNode.StructNode.FrameListNode[drawCall.StructNode.frameIndex];
                RWGeometry geo = clumpNode.GeometryListNode.GeometryListNode[drawCall.StructNode.geometryIndex];
                RWSkinPlugin skin = FindNode(clumpNode.GeometryListNode.GeometryListNode[drawCall.StructNode.geometryIndex].ExtensionNode.Children, RWType.SkinNode) as RWSkinPlugin;
                foreach (RWTriangle tri in geo.StructNode.Triangles)
                {
                    SMDVertex[] vertices = new SMDVertex[3];
                    for (int i = 0; i < 3; i++)
                    {
                        ushort vIdx = tri[i];

                        Vector3 pos = geo.StructNode.Positions[vIdx];
                        pos = Vector3.Transform(pos, rwFrame.WorldMatrix);

                        Vector3 nrm = geo.StructNode.Normals[vIdx];
                        nrm = Vector3.TransformNormal(nrm, rwFrame.WorldMatrix);

                        Vector2 uv = new Vector2();
                        if (geo.StructNode.TexCoordCount > 0)
                            uv = new Vector2(geo.StructNode.TexCoordSets[0][vIdx].X, geo.StructNode.TexCoordSets[0][vIdx].Y * -1);

                        SMDLink[] links;
                        if (skin != null)
                        {
                            int usedBoneCount = skin.VertexBoneWeights[vIdx].Count(w => w != 0.0f);
                            links = new SMDLink[usedBoneCount];
                            for (int j = 0; j < usedBoneCount; j++)
                            {
                                links[j].BoneID = (int)clumpNode.FrameListNode.HierarchyIndexToFrameIndex(skin.VertexBoneIndices[vIdx][j]);
                                links[j].Weight = skin.VertexBoneWeights[vIdx][j];
                            }
                        }
                        else
                        {
                            links = new SMDLink[1];
                            links[0].BoneID = drawCall.StructNode.frameIndex;
                            links[0].Weight = 1.0f;
                        }

                        vertices[i] = new SMDVertex(
                            drawCall.StructNode.frameIndex, pos, nrm, uv, links);
                    }

                    string textureName = "NoTexture";
                    if (geo.MaterialListNode.Materials[tri.MatID].TextureReferenceNode != null)
                        textureName = geo.MaterialListNode.Materials[tri.MatID].TextureReferenceNode.Name;

                    //textureName = drawCall.StructNode.GeometryIndex + "_" + rwTriangle.MatID;

                    smd.Triangles.Add(new SMDTriangle(textureName, vertices));
                }
            }
            return smd;
        }

        */
    }
}
