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
            Assimp.Scene aiScene = new Assimp.Scene();

            int drawCallIdx = 0;
            int materialIdx = 0;
            int totalSplitIdx = 0;
            List<int> meshStartIndices = new List<int>();
            foreach (RwAtomicNode drawCall in clumpNode.Atomics)
            {
                meshStartIndices.Add(totalSplitIdx);
                var mesh = clumpNode.GeometryList[drawCall.GeometryIndex];
                var node = clumpNode.FrameList[drawCall.FrameIndex];
                var skinPlugin = (RwSkinNode)mesh.ExtensionNodes.SingleOrDefault(x => x.Id == RwNodeId.RwSkinNode);

                int splitIdx = 0;
                foreach (RwMesh split in mesh.MeshListNode.MaterialMeshes)
                {
                    Assimp.Mesh aiMesh = new Assimp.Mesh(Assimp.PrimitiveType.Triangle)
                    {
                        Name = $"AtomicNode{drawCallIdx.ToString("00")}_Split{splitIdx.ToString("00")}",
                        MaterialIndex = split.MaterialIndex + materialIdx
                    };

                    // get split indices
                    int[] indices = split.Indices;
                    if (mesh.MeshListNode.PrimitiveType == RwPrimitiveType.TriangleStrip)
                        indices = MeshUtilities.ToTriangleList(indices, false);

                    // pos & nrm
                    for (int i = 0; i < indices.Length; i++)
                    {
                        if (mesh.HasVertices)
                        {
                            var vert = Vector3.Transform(mesh.Vertices[indices[i]], node.WorldTransform);
                            aiMesh.Vertices.Add(vert.ToAssimpVector3D());
                        }
                        if (mesh.HasNormals)
                        {
                            var nrm = Vector3.TransformNormal(mesh.Normals[indices[i]], node.WorldTransform);
                            aiMesh.Normals.Add(nrm.ToAssimpVector3D());
                        }
                    }

                    // tex coords
                    if (mesh.HasTexCoords)
                    {
                        for (int i = 0; i < mesh.TextureCoordinateChannelCount; i++)
                        {
                            List<Assimp.Vector3D> texCoordChannel = new List<Assimp.Vector3D>();

                            for (int j = 0; j < indices.Length; j++)
                            {
                                texCoordChannel.Add(mesh.TextureCoordinateChannels[i][indices[j]].ToAssimpVector3D(0));
                            }

                            aiMesh.TextureCoordinateChannels[i] = texCoordChannel;
                        }
                    }

                    // colors
                    if (mesh.HasColors)
                    {
                        List<Assimp.Color4D> vertColorChannel = new List<Assimp.Color4D>();

                        for (int i = 0; i < indices.Length; i++)
                        {
                            var color = mesh.Colors[indices[i]];
                            vertColorChannel.Add(new Assimp.Color4D(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f));
                        }

                        aiMesh.VertexColorChannels[0] = vertColorChannel;
                    }

                    // generate temporary face indices
                    int[] tempIndices = new int[aiMesh.VertexCount];
                    for (int i = 0; i < aiMesh.VertexCount; i++)
                        tempIndices[i] = i;

                    aiMesh.SetIndices(tempIndices, 3);

                    // vertex weights
                    if (skinPlugin != null)
                    {
                        var boneDictionary = new Dictionary<int, Assimp.Bone>();

                        for (int i = 0; i < indices.Length; i++)
                        {
                            var skinWeights = skinPlugin.VertexBoneWeights[indices[i]];
                            var skinBoneIds = skinPlugin.VertexBoneIndices[indices[i]]
                                .Select(x => clumpNode.FrameList.GetFrameIndexByHierarchyIndex(x))
                                .ToList();

                            for (int j = 0; j < 4; j++)
                            {
                                if (skinWeights[j] != 0.0f)
                                {
                                    Assimp.VertexWeight vertexWeight =
                                        new Assimp.VertexWeight(indices[i], skinWeights[j]);

                                    if (!boneDictionary.ContainsKey(skinBoneIds[j]))
                                    {
                                        boneDictionary[skinBoneIds[j]] =
                                            new Assimp.Bone(
                                                clumpNode.FrameList[skinBoneIds[j]].HAnimFrameExtensionNode.NameId.ToString(),
                                                Assimp.Matrix3x3.Identity, new[] {vertexWeight});
                                    }
                                    else
                                    {
                                        boneDictionary[skinBoneIds[j]].VertexWeights.Add(vertexWeight);
                                    }
                                }
                            }
                        }

                        var bones = boneDictionary.Values.ToList();
                        foreach (var bone in bones)
                            aiMesh.Bones.Add(bone);
                    }

                    // add the mesh to the listNode
                    aiScene.Meshes.Add(aiMesh);

                    splitIdx++;
                }

                totalSplitIdx += splitIdx;

                foreach (RwMaterial mat in mesh.Materials)
                {
                    Assimp.Material aiMaterial = new Assimp.Material();
                    aiMaterial.AddProperty(new Assimp.MaterialProperty(Assimp.Unmanaged.AiMatKeys.NAME, "MaterialNode" + (materialIdx++).ToString("00")));

                    if (mat.IsTextured)
                    {
                        aiMaterial.AddProperty(new Assimp.MaterialProperty(Assimp.Unmanaged.AiMatKeys.TEXTURE_BASE, mat.TextureReferenceNode.ReferencedTextureName + ".png", Assimp.TextureType.Diffuse, 0));
                    }

                    aiScene.Materials.Add(aiMaterial);
                }

                drawCallIdx++;
            }

            // store node lookup
            Dictionary<RwFrame, Assimp.Node> nodeLookup = new Dictionary<RwFrame, Assimp.Node>();

            // first create the root node
            var rootNode = new Assimp.Node("Root") {Transform = clumpNode.FrameList[0].Transform.ToAssimpMatrix4x4()};
            nodeLookup.Add(clumpNode.FrameList[0], rootNode);

            for (int i = 1; i < clumpNode.FrameList.Count - 1; i++)
            {
                var node = clumpNode.FrameList[i];
                string name = node.HAnimFrameExtensionNode.NameId.ToString();

                var aiNode = new Assimp.Node(name) {Transform = node.Transform.ToAssimpMatrix4x4()};

                // get the associated meshes for this node
                var drawCalls = clumpNode.Atomics.FindAll(dc => dc.FrameIndex == i);
                foreach (var drawCall in drawCalls)
                {
                    for (int j = 0; j < clumpNode.GeometryList[drawCall.GeometryIndex].MaterialCount; j++)
                    {
                        aiNode.MeshIndices.Add(meshStartIndices[clumpNode.Atomics.IndexOf(drawCall)] + j);
                    }
                }

                nodeLookup[node.Parent].Children.Add(aiNode);
                nodeLookup.Add(node, aiNode);
            }

            aiScene.RootNode = rootNode;

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

            if (AtomicCount > 0)
            {
                mExtensionNodeNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
            }
            else
            {
                mExtensionNodeNode = new RwExtensionNode(this);
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
                var rootNode = scene.RootNode.Children.SingleOrDefault(x => x.MeshIndices.Contains(i));

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

                    for (int j = 0; j < assimpMesh.TextureCoordinateChannelCount; j++)
                    {
                        for (int k = 0; k < assimpMesh.TextureCoordinateChannels[j].Count; k++)
                        {
                            var vector = assimpMesh.TextureCoordinateChannels[j][k];
                            assimpMesh.TextureCoordinateChannels[j][k] =
                                new Assimp.Vector3D(vector.X, -vector.Y, 0);
                        }
                    }
                }

                var geometryNode = new RwGeometryNode(this, assimpMesh, scene.Materials[assimpMesh.MaterialIndex], FrameList, inverseBoneMatrices, out bool singleWeight);
                GeometryList.Add( geometryNode );

                var atomicNode = new RwAtomicNode(this, 0, i, 5);
                if (singleWeight)
                {
                    atomicNode.FrameIndex = FrameList.GetFrameIndexByName(assimpMesh.Bones[0].Name);
                }

                Atomics.Add( atomicNode );
            }

            mStructNode = new RwClumpStructNode(this);
        }

        private Assimp.Matrix4x4 GetWorldTransform(Assimp.Node node)
        {
            var matrix = node.Transform;
            if (node.Parent != null)
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
