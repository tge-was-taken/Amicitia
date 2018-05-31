namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Numerics;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare mesh and all of its corresponding data structures.
    /// </summary>
    public class RwGeometryNode : RwNode
    {
        private RwGeometryStructNode mStructNode;
        private RwExtensionNode mExtensionNode;

        #region Properties

        /*****************************************/
        /* RWGeometryStruct forwarded properties */
        /*****************************************/

        /// <summary>
        /// Gets if the mesh contains any vertices.
        /// </summary>
        public bool HasVertices
        {
            get { return mStructNode.HasVertices; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex normals.
        /// </summary>
        public bool HasNormals
        {
            get { return mStructNode.HasNormals; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex colors.
        /// </summary>
        public bool HasColors
        {
            get { return mStructNode.HasColors; }
        }

        /// <summary>
        /// Gets if the mesh contains any vertex texture coordinates.
        /// </summary>
        public bool HasTextureCoordinates
        {
            get { return mStructNode.HasTexCoords; }
        }

        /// <summary>
        /// Gets the <see cref="RwGeometryFlags"/> of the mesh.
        /// </summary>
        public RwGeometryFlags Flags
        {
            get { return mStructNode.Flags; }
            internal set { mStructNode.Flags = value; }
        }

        /// <summary>
        /// Gets the number of texture coordinate channels in the mesh.
        /// </summary>
        public int TextureCoordinateChannelCount
        {
            get { return mStructNode.TextureCoordinateChannelCount; }
        }

        /// <summary>
        /// Gets the <see cref="RwGeometryNativeFlag"/> of the mesh.
        /// </summary>
        public RwGeometryNativeFlag NativeFlag
        {
            get { return mStructNode.NativeFlag; }
        }

        /// <summary>
        /// Gets the number of triangles in the mesh.
        /// </summary>
        public int TriangleCount
        {
            get { return mStructNode.TriangleCount; }
        }

        /// <summary>
        /// Gets the number of vertices in the mesh.
        /// </summary>
        public int VertexCount
        {
            get { return mStructNode.VertexCount; }
        }

        /// <summary>
        /// Gets the array of vertex colors in the mesh.
        /// </summary>
        public Color[] Colors
        {
            get { return mStructNode.Colors; }
        }

        /// <summary>
        /// Gets the array of texture coordinate channels in the mesh.
        /// </summary>
        public Vector2[][] TextureCoordinateChannels
        {
            get { return mStructNode.TextureCoordinateChannels; }
        }

        /// <summary>
        /// Gets the array of triangles in the mesh.
        /// </summary>
        public RwTriangle[] Triangles
        {
            get { return mStructNode.Triangles; }
        }

        /// <summary>
        /// Gets the bounding sphere of the mesh.
        /// </summary>
        public RwBoundingSphere BoundingSphere
        {
            get { return mStructNode.BoundingSphere; }
            set { mStructNode.BoundingSphere = value; }
        }

        /// <summary>
        /// Gets the array of vertices in the mesh.
        /// </summary>
        public Vector3[] Vertices
        {
            get { return mStructNode.Vertices; }
        }

        /// <summary>
        /// Gets the array of vertex normals in the mesh.
        /// </summary>
        public Vector3[] Normals
        {
            get { return mStructNode.Normals; }
        }

        /***************************************/
        /* RWMaterialList forwarded properties */
        /***************************************/

        /// <summary>
        /// Gets the number of materials in the mesh.
        /// </summary>
        public int MaterialCount
        {
            get { return Materials.Count; }
        }

        /// <summary>
        /// Gets the array of materials in the mesh.
        /// </summary>
        public RwMaterialListNode Materials { get; }

        /************************************/
        /* RWExtension forwarded properties */
        /************************************/

        /// <summary>
        /// Gets the mesh material split data.
        /// </summary>
        public RwMeshListNode MeshListNode
        {
            get
            {
                int matSplitIdx;
                if (mExtensionNode.TryGetExtensionIndexOfType(RwNodeId.RwMeshListNode, out matSplitIdx))
                {
                    return (RwMeshListNode)mExtensionNode.Children[matSplitIdx];
                }

                return null;
                /*
                else
                {
                    var splitData = new RWGeometryMaterialSplitMeshData(this, RWPrimitiveType.TriangleStrip, _extension);
                    return splitData;
                }
                */
            }
            set
            {
                int matSplitIdx;
                if (mExtensionNode.TryGetExtensionIndexOfType(RwNodeId.RwMeshListNode, out matSplitIdx))
                {
                    mExtensionNode.Children[matSplitIdx] = value;
                }
                else
                {
                    value.Parent = mExtensionNode;
                    mExtensionNode.Children.Add(value);
                }
            }
        }

        public RwSkinNode SkinNode
        {
            get { return (RwSkinNode) mExtensionNode.Children.Find(x => x.Id == RwNodeId.RwSkinNode); }
            set
            {
                int index = mExtensionNode.Children.FindIndex(x => x.Id == RwNodeId.RwSkinNode);
                if (index != -1)
                {
                    mExtensionNode.Children[index] = value;
                }
                else
                {
                    value.Parent = mExtensionNode;
                    mExtensionNode.Children.Add(value);
                }
            }
        }

        /// <summary>
        /// Gets the extension nodes of the mesh.
        /// </summary>
        public List<RwNode> ExtensionNodes
        {
            get { return mExtensionNode.Children; }
        }

        /****************************************************************************************/
        /* TODO: add the skin plugin and mesh strip plugin here instead of the extension itself */
        /****************************************************************************************/

        #endregion

        /// <summary>
        /// Initializer only to be called <see cref="RwNodeFactory"/>
        /// </summary>
        internal RwGeometryNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwGeometryStructNode>(this, reader);
            Materials = RwNodeFactory.GetNode<RwMaterialListNode>(this, reader);
            mExtensionNode = RwNodeFactory.GetNode<RwExtensionNode>(this, reader);
        }

        public RwGeometryNode(RwNode parent, Assimp.Mesh mesh, Assimp.Material material, RwFrameListNode frameList, Matrix4x4[] inverseBoneMatrices, out bool singleWeight)
            : base(RwNodeId.RwGeometryNode, parent)
        {
            bool forceSingleWeight = inverseBoneMatrices == null;
            mStructNode = new RwGeometryStructNode(this, mesh, frameList, forceSingleWeight, out byte[][] skinBoneIndices, out float[][] skinBoneWeights, out singleWeight);
            Materials = new RwMaterialListNode(this, material);

            var materialSplit = new RwMeshListNode( this );
            if ( !singleWeight )
            {
                var skinPlugin = new RwSkinNode( skinBoneIndices, skinBoneWeights, inverseBoneMatrices );
                mExtensionNode = new RwExtensionNode( this, skinPlugin, materialSplit );
            }
            else
            {
                mExtensionNode = new RwExtensionNode( this, materialSplit );
            }
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.Write(writer);
            Materials.Write(writer);
            mExtensionNode.Write(writer);
        }
    }
}