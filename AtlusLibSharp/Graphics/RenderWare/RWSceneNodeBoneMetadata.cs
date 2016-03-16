namespace AtlusLibSharp.Graphics.RenderWare
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class RWRootBoneInfo
    {
        private RWRootBoneFlags _flags;
        private List<RWBoneHierarchyNode> _hierarchy;

        #region Properties

        public RWRootBoneFlags Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public int HierarchyNodeCount
        {
            get { return _hierarchy.Count; }
        }

        public List<RWBoneHierarchyNode> HierarchyNodes
        {
            get { return _hierarchy; }
            internal set { _hierarchy = value; }
        }

        #endregion

        public RWRootBoneInfo(RWRootBoneFlags flags, List<RWBoneHierarchyNode> hierarchyNodes)
        {
            _flags = flags;
            _hierarchy = hierarchyNodes;
        }
    }

    public class RWSceneNodeBoneMetadata : RWNode
    {
        private const int VERSION = 0x100;
        private const int KEYFRAME_SIZE = 36;

        private int _nameID;
        private RWRootBoneInfo _rootInfo;

        #region Properties

        public int BoneNameID
        {
            get { return _nameID; }
        }

        public bool IsRootBone
        {
            get { return _rootInfo != null; }
        }

        public RWRootBoneInfo RootInfo
        {
            get { return _rootInfo; }
        }

        #endregion

        #region Constructors

        public RWSceneNodeBoneMetadata(int boneName)
            : base(RWNodeType.SceneNodeBoneMetadata)
        {
            _nameID = boneName;
        }

        public RWSceneNodeBoneMetadata(int boneName, RWRootBoneInfo rootInfo)
            : base(RWNodeType.SceneNodeBoneMetadata)
        {
            _nameID = boneName;

            if (rootInfo == null)
                throw new ArgumentNullException("rootInfo");

            _rootInfo = rootInfo;
        }

        internal RWSceneNodeBoneMetadata(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
                : base(header)
        {
            int version = reader.ReadInt32();

            if (version != VERSION)
            {
                throw new NotImplementedException("Unexpected version for RWHierarchyAnimPlugin");
            }

            _nameID = reader.ReadInt32();
            int numNodes = reader.ReadInt32();

            if (numNodes == 0)
                return;

            RWRootBoneFlags flags = (RWRootBoneFlags)reader.ReadUInt32();
            int keyFrameSize = reader.ReadInt32();
            List< RWBoneHierarchyNode> hierarchyNodes = new List<RWBoneHierarchyNode>(numNodes);

            for (int i = 0; i < numNodes; i++)
            {
                hierarchyNodes.Add(new RWBoneHierarchyNode(reader));
            }

            _rootInfo = new RWRootBoneInfo(flags, hierarchyNodes);
        }

        #endregion

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(VERSION);
            writer.Write(_nameID);

            if (!IsRootBone)
            {
                writer.Write(0);
                return;
            }

            writer.Write((uint)_rootInfo.Flags);
            writer.Write(KEYFRAME_SIZE);

            for (int i = 0; i < _rootInfo.HierarchyNodeCount; i++)
                _rootInfo.HierarchyNodes[i].InternalWrite(writer);
        }
    }
}
