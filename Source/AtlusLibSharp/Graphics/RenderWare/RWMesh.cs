namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Numerics;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare mesh and all of its corresponding data structures.
    /// </summary>
    public class RWMesh : RWNode
    {
        private RWMeshStruct _struct;
        private RWMaterialList _materialList;
        private RWExtension _extension;

        #region Properties

        /*****************************************/
        /* RWMeshStruct forwarded properties */
        /*****************************************/

        /// <summary>
        /// Gets if the mesh contains any vertices.
        /// </summary>
        public bool HasVertices
        {
            get { return _struct.HasVertices; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex normals.
        /// </summary>
        public bool HasNormals
        {
            get { return _struct.HasNormals; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex colors.
        /// </summary>
        public bool HasColors
        {
            get { return _struct.HasColors; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex texture coordinates.
        /// </summary>
        public bool HasTexCoords
        {
            get { return _struct.HasTexCoords; }
        }

        /// <summary>
        /// Gets the <see cref="RWGeometryFlags"/> of the mesh.
        /// </summary>
        public RWGeometryFlags Flags
        {
            get { return _struct.Flags; }
        }

        /// <summary>
        /// Gets the number of texture coordinate channels in the mesh.
        /// </summary>
        public int TextureCoordinateChannelCount
        {
            get { return _struct.TextureCoordinateChannelCount; }
        }

        /// <summary>
        /// Gets the <see cref="RWGeometryNativeFlag"/> of the mesh.
        /// </summary>
        public RWGeometryNativeFlag NativeFlag
        {
            get { return _struct.NativeFlag; }
        }

        /// <summary>
        /// Gets the number of triangles in the mesh.
        /// </summary>
        public int TriangleCount
        {
            get { return _struct.TriangleCount; }
        }

        /// <summary>
        /// Gets the number of vertices in the mesh.
        /// </summary>
        public int VertexCount
        {
            get { return _struct.VertexCount; }
        }

        /// <summary>
        /// Gets the array of vertex colors in the mesh.
        /// </summary>
        public Color[] Colors
        {
            get { return _struct.Colors; }
        }

        /// <summary>
        /// Gets the array of texture coordinate channels in the mesh.
        /// </summary>
        public Vector2[][] TextureCoordinateChannels
        {
            get { return _struct.TextureCoordinateChannels; }
        }

        /// <summary>
        /// Gets the array of triangles in the mesh.
        /// </summary>
        public RWTriangle[] Triangles
        {
            get { return _struct.Triangles; }
        }

        /// <summary>
        /// Gets the bounding sphere of the mesh.
        /// </summary>
        public RWBoundingSphere BoundingSphere
        {
            get { return _struct.BoundingSphere; }
        }

        /// <summary>
        /// Gets the array of vertices in the mesh.
        /// </summary>
        public Vector3[] Vertices
        {
            get { return _struct.Vertices; }
        }

        /// <summary>
        /// Gets the array of vertex normals in the mesh.
        /// </summary>
        public Vector3[] Normals
        {
            get { return _struct.Normals; }
        }

        /***************************************/
        /* RWMaterialList forwarded properties */
        /***************************************/

        /// <summary>
        /// Gets the number of materials in the mesh.
        /// </summary>
        public int MaterialCount
        {
            get { return _materialList.MaterialCount; }
        }

        /// <summary>
        /// Gets the array of materials in the mesh.
        /// </summary>
        public RWMaterial[] Materials
        {
            get { return _materialList.Materials; }
        }

        /************************************/
        /* RWExtension forwarded properties */
        /************************************/

        /// <summary>
        /// Gets the mesh material split data.
        /// </summary>
        public RWMeshMaterialSplitData MaterialSplitData
        {
            get
            {
                int matSplitIdx;
                if (_extension.TryGetExtensionIndexOfType(RWNodeType.MeshMaterialSplitList, out matSplitIdx))
                {
                    return (RWMeshMaterialSplitData)_extension.Children[matSplitIdx];
                }
                else
                {
                    var splitData = new RWMeshMaterialSplitData(this, RWPrimitiveType.TriangleStrip, _extension);
                    return splitData;
                }
            }
            internal set
            {
                int matSplitIdx;
                if (_extension.TryGetExtensionIndexOfType(RWNodeType.MeshMaterialSplitList, out matSplitIdx))
                {
                    _extension.Children[matSplitIdx] = value;
                }
                else
                {
                    value.Parent = _extension;
                }
            }
        }

        /// <summary>
        /// Gets the extension nodes of the mesh.
        /// </summary>
        public List<RWNode> ExtensionNodes
        {
            get { return _extension.Children; }
        }

        /****************************************************************************************/
        /* TODO: add the skin plugin and mesh strip plugin here instead of the extension itself */
        /****************************************************************************************/

        #endregion

        /// <summary>
        /// Initializer only to be called <see cref="RWNodeFactory"/>
        /// </summary>
        internal RWMesh(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWMeshStruct>(this, reader);
            _materialList = RWNodeFactory.GetNode<RWMaterialList>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /*
        public static RWGeometry FromSMD(RWScene refScene, string filename)
        {
            SMDFile smd = new SMDFile(filename);
            Vector3[] pos = new Vector3[smd.Triangles.Count * 3];
            Vector3[] nrm = new Vector3[smd.Triangles.Count * 3];
            Vector2[] uv = new Vector2[smd.Triangles.Count * 3];
            byte[][] skinBoneIndices = new byte[smd.Triangles.Count * 3][];
            float[][] skinBoneWeights = new float[smd.Triangles.Count * 3][];
            List<string> textureList = new List<string>();
            List<ushort> textureIDList = new List<ushort>();

            int vIdx = -1;
            foreach (SMDTriangle smdTri in smd.Triangles)
            {
                string materialName = Path.GetFileNameWithoutExtension(smdTri.MaterialName);
                if (!textureList.Contains(materialName))
                    textureList.Add(materialName);
                textureIDList.Add((ushort)textureList.IndexOf(materialName));

                foreach (SMDVertex smdVtx in smdTri.Vertices)
                {
                    ++vIdx;
                    pos[vIdx] = smdVtx.Position;
                    nrm[vIdx] = smdVtx.Normal;
                    //pos[vIdx] = Vector3.Transform(smdVtx.Position, refScene.FrameList.Struct.Frames[2].WorldMatrix.Inverted());
                    //nrm[vIdx] = Vector3.TransformNormal(smdVtx.Normal, refScene.FrameList.Struct.Frames[2].WorldMatrix.Inverted());
                    uv[vIdx] = new Vector2(smdVtx.UV.X, smdVtx.UV.Y);
                    skinBoneIndices[vIdx] = new byte[4];
                    skinBoneWeights[vIdx] = new float[4];

                    float weightSum = 0.0f;

                    SMDLink[] links = smdVtx.Links;

                    if (smdVtx.LinkCount > 4)
                    {
                        links = smdVtx.Links.OrderBy(l => l.Weight).ToArray();
                    }

                    for (int i = 0; i < smdVtx.LinkCount; i++)
                    {
                        if (i == 4)
                            break;

                        uint boneNameID = uint.Parse(smd.Nodes[smdVtx.Links[i].BoneID].NodeName);
                        skinBoneIndices[vIdx][i] = (byte)refScene.FrameList.GetHierarchyIndexByNameID(boneNameID);
                        skinBoneWeights[vIdx][i] = smdVtx.Links[i].Weight;
                        weightSum += smdVtx.Links[i].Weight;
                    }

                    if (weightSum != 1.0f)
                    {
                        float addWeight = (1.0f - weightSum) / 4;
                        for (int i = 0; i < 4; i++)
                        {
                            skinBoneWeights[vIdx][i] += addWeight;
                        }
                    }

                }
            }

            Vector2[][] uvs = new Vector2[1][];
            uvs[0] = uv;

            RWGeometry geometry = new RWGeometry
            {
                Struct = new RWGeometryStruct(ref skinBoneIndices, ref skinBoneWeights, pos, nrm, textureIDList.ToArray(), uvs),
                MaterialList = new RWMaterialList(textureList.ToArray()),
            };

            RWSkinPlugin skin = new RWSkinPlugin(refScene.FrameList, geometry, skinBoneIndices, skinBoneWeights);

            geometry.Extension = new RWExtension(skin);

            return geometry;
        }
        */

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _materialList.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}