namespace AtlusLibSharp.Graphics.RenderWare
{
    using Utilities;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare model scene containing meshes, draw calls, lights and cameras.
    /// </summary>
    public class RWScene : RWNode
    {
        private RWSceneStruct _structNode;
        private RWSceneNodeList _frameListNode;
        private RWMeshList _meshListNode;
        private List<RWDrawCall> _drawCalls;
        private RWExtension _extensionNode;

        #region Properties

        /// <summary>
        /// Gets the list of draw calls in the scene.
        /// </summary>
        public List<RWDrawCall> DrawCalls
        {
            get { return _drawCalls; }
        }

        /// <summary>
        /// Gets the number of draw calls in the scene.
        /// </summary>
        public int DrawCallCount
        {
            get { return _drawCalls.Count; }
        }

        /// <summary>
        /// Gets if the scene has any draw calls.
        /// </summary>
        public bool HasDrawCalls
        {
            get { return DrawCallCount > 0; }
        }

        /// <summary>
        /// Gets the number of meshes in the scene.
        /// </summary>
        public int MeshCount
        {
            get { return Meshes.Count; }
        }

        /// <summary>
        /// Gets if the scene has any meshes.
        /// </summary>
        public bool HasMeshes
        {
            get { return MeshCount > 0; }
        }

        /**************************************/
        /* RWSceneStruct forwarded properties */
        /**************************************/

        /// <summary>
        /// Gets the number of lights in the scene.
        /// </summary>
        public int LightCount
        {
            get { return _structNode.LightCount; }
        }

        /// <summary>
        /// Gets the number of cameras in the scene.
        /// </summary>
        public int CameraCount
        {
            get { return _structNode.CameraCount; }
        }

        /****************************************/
        /* RWSceneNodeList forwarded properties */
        /****************************************/

        /// <summary>
        /// Gets the list of scene nodes in the scene.
        /// </summary>
        public List<RWSceneNode> Nodes
        {
            get { return _frameListNode.SceneNodes; }
        }

        /// <summary>
        /// Gets the scene node containing the bone hierarchy used in animations.
        /// </summary>
        public RWSceneNode AnimationRootBone
        {
            get { return _frameListNode.AnimationRootNode; }
        }

        /***********************************/
        /* RWMeshList forwarded properties */
        /***********************************/

        /// <summary>
        /// Gets the list of meshes in this scene.
        /// </summary>
        public List<RWMesh> Meshes
        {
            get { return _meshListNode.Meshes; }
        }

        /************************************/
        /* RWExtension forwarded properties */
        /************************************/

        /// <summary>
        /// Gets the extension nodes for this scene.
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extensionNode.Children; }
        }

        #endregion

        public static Assimp.Scene ToAssimpScene(RWScene scene)
        {
            Assimp.Scene aiScene = new Assimp.Scene();
            aiScene.RootNode = new Assimp.Node("RootNode");

            int geoIdx = 0;
            foreach (RWDrawCall drawCall in scene.DrawCalls)
            {
                var mesh = scene.Meshes[drawCall.MeshIndex];
                Assimp.Mesh aiMesh = new Assimp.Mesh("Mesh" + (geoIdx++).ToString("00"), Assimp.PrimitiveType.Triangle);

                aiMesh.MaterialIndex = 0;

                foreach (RWTriangle tri in mesh.Triangles)
                {
                    aiMesh.Faces.Add(new Assimp.Face(new int[] { tri.A, tri.B, tri.C }));
                }

                foreach (OpenTK.Vector3 vertex in mesh.Vertices)
                {
                    aiMesh.Vertices.Add(new Assimp.Vector3D(vertex.X, vertex.Y, vertex.Z));
                }

                if (mesh.HasNormals)
                {
                    foreach (OpenTK.Vector3 normal in mesh.Normals)
                    {
                        aiMesh.Normals.Add(new Assimp.Vector3D(normal.X, normal.Y, normal.Z));
                    }
                }

                for (int i = 0; i < mesh.TextureCoordinateChannelCount; i++)
                {
                    List<Assimp.Vector3D> texCoordChannel = new List<Assimp.Vector3D>();

                    foreach (OpenTK.Vector2 texCoord in mesh.TextureCoordinateChannels[i])
                    {
                        texCoordChannel.Add(new Assimp.Vector3D(texCoord.X, texCoord.Y, 0));
                    }

                    aiMesh.TextureCoordinateChannels[i] = texCoordChannel;
                }

                if (mesh.HasColors)
                {
                    List<Assimp.Color4D> vertColorChannel = new List<Assimp.Color4D>();

                    foreach (Color color in mesh.Colors)
                    {
                        vertColorChannel.Add(new Assimp.Color4D(color.R * 255, color.G * 255, color.B * 255, color.A * 255));
                    }

                    aiMesh.VertexColorChannels[0] = vertColorChannel;
                }

                int materialIdx = 0;
                foreach (RWMaterial mat in mesh.Materials)
                {
                    Assimp.Material aiMaterial = new Assimp.Material();
                    aiMaterial.AddProperty(new Assimp.MaterialProperty(Assimp.Unmanaged.AiMatKeys.NAME, "Material" + (materialIdx++).ToString("00")));

                    if (mat.IsTextured)
                    {
                        aiMaterial.AddProperty(new Assimp.MaterialProperty(Assimp.Unmanaged.AiMatKeys.TEXTURE_BASE, mat.TextureReference.ReferencedTextureName + ".png", Assimp.TextureType.Diffuse, 0));
                    }

                    aiScene.Materials.Add(aiMaterial);
                }

                aiScene.Meshes.Add(aiMesh);
            }

            foreach (RWDrawCall drawCall in scene.DrawCalls)
            {
                Assimp.Node node = new Assimp.Node();
                node.Name = aiScene.Meshes[drawCall.MeshIndex].Name;
                node.MeshIndices.Add(drawCall.MeshIndex);
                node.Transform = scene.Nodes[drawCall.NodeIndex].Transform.ToAssimpMatrix4x4();
                aiScene.RootNode.Children.Add(node);
            }

            return aiScene;
        }

        /// <summary>
        /// Initialize a new RenderWare Scene using a <see cref="Assimp.Scene"/>.
        /// </summary>
        /// <param name="scene"></param>
        public RWScene(Assimp.Scene scene)
            : base(RWNodeType.Scene)
        {
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWScene(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _structNode = RWNodeFactory.GetNode<RWSceneStruct>(this, reader);
            _frameListNode = RWNodeFactory.GetNode<RWSceneNodeList>(this, reader);
            _meshListNode = RWNodeFactory.GetNode<RWMeshList>(this, reader);
            _drawCalls = new List<RWDrawCall>(_structNode.DrawCallCount);

            for (int i = 0; i < _structNode.DrawCallCount; i++)
            {
                _drawCalls.Add(RWNodeFactory.GetNode<RWDrawCall>(this, reader));
            }

            if (DrawCallCount > 0)
            {
                _extensionNode = RWNodeFactory.GetNode<RWExtension>(this, reader);
            }
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _structNode.InternalWrite(writer);
            _frameListNode.InternalWrite(writer);
            _meshListNode.InternalWrite(writer);

            foreach (RWDrawCall drawCall in DrawCalls)
            {
                drawCall.InternalWrite(writer);
            }

            if (HasDrawCalls)
            {
                _extensionNode.InternalWrite(writer);
            }
        }

        /*

        public static OBJFile ToOBJFile(RWScene scene)
        {
            OBJFile obj = new OBJFile();
            obj.MaterialLibrary = new OBJMaterialLibrary();
            foreach (RWDrawCall drawCall in scene.DrawCallList)
            {
                RWFrame frame = scene.FrameList.Struct.Frames[drawCall.Struct.frameIndex];
                RWGeometry geo = scene.GeometryList.GeometryList[drawCall.Struct.geometryIndex];

                Vector3[] transPositions = new Vector3[geo.Struct.Positions.Length];
                for (int i = 0; i < geo.Struct.Positions.Length; i++)
                    transPositions[i] = Vector3.Transform(geo.Struct.Positions[i], frame.WorldMatrix);

                Vector3[] transNormals = new Vector3[geo.Struct.Normals.Length];
                for (int i = 0; i < geo.Struct.Normals.Length; i++)
                    transNormals[i] = Vector3.TransformNormal(geo.Struct.Normals[i], frame.WorldMatrix);

                Vector2[] uvs = geo.Struct.TexCoordSets.Length > 0 ? geo.Struct.TexCoordSets[0] : null;
                OBJFace[] faces = new OBJFace[geo.Struct.Triangles.Length];
                for (int i = 0; i < geo.Struct.Triangles.Length; i++)
                {
                    List<int> indices = new List<int> { geo.Struct.Triangles[i].A + 1, geo.Struct.Triangles[i].B + 1, geo.Struct.Triangles[i].C + 1 };
                    faces[i] = new OBJFace("geometry_" + drawCall.Struct.geometryIndex + "_material_" + geo.Struct.Triangles[i].MatID,
                        indices, // Position Indices
                        uvs != null ? indices : null, // UV Indices
                        indices); // Normal Indices
                }
                obj.Groups.Add(new OBJGroup("drawCall_" + scene.DrawCallList.IndexOf(drawCall), transPositions, uvs, transNormals, faces));
                int idx = -1;
                foreach (RWMaterial material in geo.MaterialList.Materials)
                {
                    idx++;
                    OBJMaterial objMat = new OBJMaterial("geometry_" + drawCall.Struct.geometryIndex + "_material_" + idx);
                    objMat.SetMap(OBJMapType.Diffuse, material.TextureReference != null ? material.TextureReference.Name : "");
                    obj.MaterialLibrary.Materials.Add(objMat);
                }
            }
            return obj;
        }

        public static SMDFile ToSMDFile(RWScene scene)
        {
            SMDFile smd = new SMDFile();
            SMDFrame frame = new SMDFrame(0);
            foreach (RWFrame rwFrame in scene.FrameList.Struct.Frames)
            {
                RWHierarchyAnimPlugin plugin = null;

                if (scene.FrameList.Extensions[rwFrame.Index].Children != null)
                    plugin = scene.FrameList.Extensions[rwFrame.Index].Children[0] as RWHierarchyAnimPlugin;

                string name = "Root";

                if (plugin != null)
                {
                    name = plugin.FrameNameID.ToString();
                    //name = scene.FrameList.GetHierarchyIndexByNameID((uint)plugin.FrameNameID) + "_" + Array.IndexOf(scene.FrameList.Struct.Frames, rwFrame);
                }

                smd.Nodes.Add(new SMDNode(smd.Nodes, rwFrame.Index, name, rwFrame.ParentIndex));
                frame.BoneFrames.Add(new SMDBoneFrame(null, rwFrame.Index, rwFrame.LocalMatrix.ExtractTranslation(), SMDFile.MatrixToEuler(rwFrame.LocalMatrix.Inverted())));
            }
            smd.Frames.Add(frame);

            foreach (RWDrawCall drawCall in scene.DrawCallList)
            {
                RWFrame rwFrame = scene.FrameList.Struct.Frames[drawCall.Struct.frameIndex];
                RWGeometry geo = scene.GeometryList.GeometryList[drawCall.Struct.geometryIndex];
                RWSkinPlugin skin = FindNode(scene.GeometryList.GeometryList[drawCall.Struct.geometryIndex].Extension.Children, RWType.SkinPlugin) as RWSkinPlugin;
                foreach (RWTriangle tri in geo.Struct.Triangles)
                {
                    SMDVertex[] vertices = new SMDVertex[3];
                    for (int i = 0; i < 3; i++)
                    {
                        ushort vIdx = tri[i];

                        Vector3 pos = geo.Struct.Positions[vIdx];
                        pos = Vector3.Transform(pos, rwFrame.WorldMatrix);

                        Vector3 nrm = geo.Struct.Normals[vIdx];
                        nrm = Vector3.TransformNormal(nrm, rwFrame.WorldMatrix);

                        Vector2 uv = new Vector2();
                        if (geo.Struct.TexCoordCount > 0)
                            uv = new Vector2(geo.Struct.TexCoordSets[0][vIdx].X, geo.Struct.TexCoordSets[0][vIdx].Y * -1);

                        SMDLink[] links;
                        if (skin != null)
                        {
                            int usedBoneCount = skin.SkinBoneWeights[vIdx].Count(w => w != 0.0f);
                            links = new SMDLink[usedBoneCount];
                            for (int j = 0; j < usedBoneCount; j++)
                            {
                                links[j].BoneID = (int)scene.FrameList.HierarchyIndexToFrameIndex(skin.SkinBoneIndices[vIdx][j]);
                                links[j].Weight = skin.SkinBoneWeights[vIdx][j];
                            }
                        }
                        else
                        {
                            links = new SMDLink[1];
                            links[0].BoneID = drawCall.Struct.frameIndex;
                            links[0].Weight = 1.0f;
                        }

                        vertices[i] = new SMDVertex(
                            drawCall.Struct.frameIndex, pos, nrm, uv, links);
                    }

                    string textureName = "NoTexture";
                    if (geo.MaterialList.Materials[tri.MatID].TextureReference != null)
                        textureName = geo.MaterialList.Materials[tri.MatID].TextureReference.Name;

                    //textureName = drawCall.Struct.GeometryIndex + "_" + rwTriangle.MatID;

                    smd.Triangles.Add(new SMDTriangle(textureName, vertices));
                }
            }
            return smd;
        }

        */
    }
}
