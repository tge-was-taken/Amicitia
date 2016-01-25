using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWClump : RWNode
    {
        // Fields
        private RWClumpStruct _struct;
        private RWFrameList _frameList;
        private RWGeometryList _geometryList;
        private List<RWAtomic> _atomicList;
        private RWExtension _extension;

        // Properties
        public RWClumpStruct Struct
        {
            get { return _struct; }
            private set
            {
                _struct = value;
                if (value == null)
                    return;
                _struct.Parent = this;
            }
        }

        public RWFrameList FrameList
        {
            get { return _frameList; }
            private set
            {
                _frameList = value;
                if (value == null)
                    return;
                _frameList.Parent = this;
            }
        }

        public RWGeometryList GeometryList
        {
            get { return _geometryList; }
            private set
            {
                _geometryList = value;
                if (value == null)
                    return;
                _geometryList.Parent = this;
            }
        }

        public List<RWAtomic> AtomicList
        {
            get { return _atomicList; }
            private set
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

        public RWExtension Extension
        {
            get { return _extension; }
            private set
            {
                _extension = value;
                if (value == null)
                    return;
                _extension.Parent = this;
            }
        }

        // Constructors
        public RWClump(RWFrameList frameList, RWGeometryList geoList, List<RWAtomic> atomicList, RWExtension extension)
            : base(RWType.Clump)
        {
            FrameList = frameList;
            GeometryList = geoList;
            AtomicList = atomicList;
            Extension = extension;
            Struct = new RWClumpStruct(this);
        }

        internal RWClump(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWClumpStruct>(this, reader);
            _frameList = RWNodeFactory.GetNode<RWFrameList>(this, reader);
            _geometryList = RWNodeFactory.GetNode<RWGeometryList>(this, reader);
            _atomicList = new List<RWAtomic>(Struct.AtomicCount);

            for (int i = 0; i < _struct.AtomicCount; i++)
            {
                _atomicList.Add(RWNodeFactory.GetNode<RWAtomic>(this, reader));
            }

            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _frameList.InternalWrite(writer);
            _geometryList.InternalWrite(writer);

            for (int i = 0; i < _struct.AtomicCount; i++)
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
