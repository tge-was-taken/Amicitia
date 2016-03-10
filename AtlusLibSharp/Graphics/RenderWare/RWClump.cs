namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Encapsulates a RenderWare clump model and all of its corresponding data structures.
    /// </summary>
    public class RWClump : RWNode
    {
        private RWClumpStruct _struct;
        private RWFrameList _frameListNode;
        private RWGeometryList _geometryListNode;
        private List<RWAtomic> _atomicList;
        private RWExtension _extension;

        #region Properties

        /// <summary>
        /// Gets the number of RenderWare atomics (draw calls) in this clump.
        /// </summary>
        public int AtomicCount
        {
            get { return _atomicList.Count; }
        }

        public RWFrameList FrameListNode
        {
            get { return _frameListNode; }
            private set
            {
                _frameListNode = value;
                if (value == null)
                    return;
                _frameListNode.Parent = this;
            }
        }

        /***************************************/
        /* RWGeometryList forwarded properties */
        /***************************************/

        /// <summary>
        /// Gets the number of geometries in this clump.
        /// </summary>
        public int GeometryCount
        {
            get { return _geometryListNode.GeometryCount; }
        }

        /// <summary>
        /// Gets the list of geometries in this clump.
        /// </summary>
        public List<RWGeometry> GeometryList
        {
            get { return _geometryListNode.GeometryList; }
        }

        /// <summary>
        /// Gets or sets the list of atomics in this clump.
        /// </summary>
        public List<RWAtomic> Atomics
        {
            get { return _atomicList; }
            set
            {
                _atomicList = value;

                if (value == null)
                    return;

                for (int i = 0; i < _atomicList.Count; i++)
                {
                    _atomicList[i].Parent = this;
                }
            }
        }

        /// <summary>
        /// Gets the extension nodes for this clump.
        /// </summary>
        public List<RWNode> Extensions
        {
            get { return _extension.Children; }
        }

        #endregion

        public RWClump(Assimp.Scene scene)
            : base(RWType.Clump)
        {
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RWNodeFactory"/>.
        /// </summary>
        internal RWClump(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWClumpStruct>(this, reader);
            _frameListNode = RWNodeFactory.GetNode<RWFrameList>(this, reader);
            _geometryListNode = RWNodeFactory.GetNode<RWGeometryList>(this, reader);
            _atomicList = new List<RWAtomic>(_struct.AtomicCount);

            for (int i = 0; i < _struct.AtomicCount; i++)
            {
                _atomicList.Add(RWNodeFactory.GetNode<RWAtomic>(this, reader));
            }

            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _frameListNode.InternalWrite(writer);
            _geometryListNode.InternalWrite(writer);

            for (int i = 0; i < _atomicList.Count; i++)
            {
                _atomicList[i].InternalWrite(writer);
            }

            _extension.InternalWrite(writer);
        }

        /*

        public static OBJFile ToOBJFile(RWClump clump)
        {
            OBJFile obj = new OBJFile();
            obj.MaterialLibrary = new OBJMaterialLibrary();
            foreach (RWAtomic atomic in clump.AtomicList)
            {
                RWFrame frame = clump.FrameList.Struct.Frames[atomic.Struct.frameIndex];
                RWGeometry geo = clump.GeometryList.GeometryList[atomic.Struct.geometryIndex];

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
                    faces[i] = new OBJFace("geometry_" + atomic.Struct.geometryIndex + "_material_" + geo.Struct.Triangles[i].MatID,
                        indices, // Position Indices
                        uvs != null ? indices : null, // UV Indices
                        indices); // Normal Indices
                }
                obj.Groups.Add(new OBJGroup("atomic_" + clump.AtomicList.IndexOf(atomic), transPositions, uvs, transNormals, faces));
                int idx = -1;
                foreach (RWMaterial material in geo.MaterialList.Materials)
                {
                    idx++;
                    OBJMaterial objMat = new OBJMaterial("geometry_" + atomic.Struct.geometryIndex + "_material_" + idx);
                    objMat.SetMap(OBJMapType.Diffuse, material.TextureReference != null ? material.TextureReference.Name : "");
                    obj.MaterialLibrary.Materials.Add(objMat);
                }
            }
            return obj;
        }

        public static SMDFile ToSMDFile(RWClump clump)
        {
            SMDFile smd = new SMDFile();
            SMDFrame frame = new SMDFrame(0);
            foreach (RWFrame rwFrame in clump.FrameList.Struct.Frames)
            {
                RWHierarchyAnimPlugin plugin = null;

                if (clump.FrameList.Extensions[rwFrame.Index].Children != null)
                    plugin = clump.FrameList.Extensions[rwFrame.Index].Children[0] as RWHierarchyAnimPlugin;

                string name = "Root";

                if (plugin != null)
                {
                    name = plugin.FrameNameID.ToString();
                    //name = clump.FrameList.GetHierarchyIndexByNameID((uint)plugin.FrameNameID) + "_" + Array.IndexOf(clump.FrameList.Struct.Frames, rwFrame);
                }

                smd.Nodes.Add(new SMDNode(smd.Nodes, rwFrame.Index, name, rwFrame.ParentIndex));
                frame.BoneFrames.Add(new SMDBoneFrame(null, rwFrame.Index, rwFrame.LocalMatrix.ExtractTranslation(), SMDFile.MatrixToEuler(rwFrame.LocalMatrix.Inverted())));
            }
            smd.Frames.Add(frame);

            foreach (RWAtomic atomic in clump.AtomicList)
            {
                RWFrame rwFrame = clump.FrameList.Struct.Frames[atomic.Struct.frameIndex];
                RWGeometry geo = clump.GeometryList.GeometryList[atomic.Struct.geometryIndex];
                RWSkinPlugin skin = FindNode(clump.GeometryList.GeometryList[atomic.Struct.geometryIndex].Extension.Children, RWType.SkinPlugin) as RWSkinPlugin;
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
                                links[j].BoneID = (int)clump.FrameList.HierarchyIndexToFrameIndex(skin.SkinBoneIndices[vIdx][j]);
                                links[j].Weight = skin.SkinBoneWeights[vIdx][j];
                            }
                        }
                        else
                        {
                            links = new SMDLink[1];
                            links[0].BoneID = atomic.Struct.frameIndex;
                            links[0].Weight = 1.0f;
                        }

                        vertices[i] = new SMDVertex(
                            atomic.Struct.frameIndex, pos, nrm, uv, links);
                    }

                    string textureName = "NoTexture";
                    if (geo.MaterialList.Materials[tri.MatID].TextureReference != null)
                        textureName = geo.MaterialList.Materials[tri.MatID].TextureReference.Name;

                    //textureName = atomic.Struct.GeometryIndex + "_" + rwTriangle.MatID;

                    smd.Triangles.Add(new SMDTriangle(textureName, vertices));
                }
            }
            return smd;
        }

        */
    }
}
