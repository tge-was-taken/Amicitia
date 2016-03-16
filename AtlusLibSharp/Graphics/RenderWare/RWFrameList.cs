using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWSceneNodeList : RWNode
    {
        // Fields
        private RWSceneNodeListStruct _struct;
        private List<RWExtension> _extensionNodes;
        private RWSceneNode _root;

        public RWSceneNode AnimationRootNode
        {
            get
            {
                if (_root == null)
                {
                    _root = GetRoot();
                }

                return _root;
            }
        }

        // Properties
        public List<RWSceneNode> SceneNodes
        {
            get { return _struct.SceneNodes; }
        }

        public int SceneNodeCount
        {
            get { return _struct.SceneNodeCount; }
        }

        public List<RWExtension> Extensions
        {
            get { return _extensionNodes; }
        }

        // Constructors
        public RWSceneNodeList(IList<RWSceneNode> frames, List<RWExtension> extensions)
            : base(RWNodeType.FrameList)
        {
            _struct = new RWSceneNodeListStruct(frames);
            _extensionNodes = extensions;
        }

        internal RWSceneNodeList(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWSceneNodeListStruct>(this, reader);
            _extensionNodes = new List<RWExtension>(_struct.SceneNodeCount);

            for (int i = 0; i < _struct.SceneNodeCount; i++)
            {
                _extensionNodes.Add(RWNodeFactory.GetNode<RWExtension>(this, reader));

                if (_extensionNodes[i].Children != null && _extensionNodes[i].Children.Count > 0)
                {
                    _struct.SceneNodes[i].BoneMetadata = _extensionNodes[i].Children[0] as RWSceneNodeBoneMetadata;
                }
            }
        }

        // Public methods
        public RWSceneNode GetFrameByHierarchyIndex(int hierarchyIndex)
        {
            return _struct.SceneNodes[ConvertHierarchyIndexToFrameIndex(hierarchyIndex)];
        }

        public int GetFrameIndexByNameID(int nameID)
        {
            int frameIdx = -1;
            foreach (RWExtension ext in Extensions)
            {
                frameIdx++;

                if (ext.Children == null || ext.Children.Count == 0)
                    continue;

                foreach (RWSceneNodeBoneMetadata plug in ext.Children)
                {
                    if (plug.BoneNameID == nameID)
                        return frameIdx;
                }
            }
            return -1;
        }

        public int GetHierarchyIndexByNameID(int nameID)
        {
            foreach (RWBoneHierarchyNode node in AnimationRootNode.BoneMetadata.RootInfo.HierarchyNodes)
            {
                if (node.FrameNameID == nameID)
                    return node.HierarchyIndex;
            }
            return -1;
        }

        public int ConvertHierarchyIndexToFrameIndex(int hierarchyIndex)
        {
            int name = -1;

            foreach (RWBoneHierarchyNode node in AnimationRootNode.BoneMetadata.RootInfo.HierarchyNodes)
            {
                if (node.HierarchyIndex == hierarchyIndex)
                    name = node.FrameNameID;
            }

            if (name == -1)
                return -1;

            int frameIndex = -1;
            foreach (RWExtension ext in Extensions)
            {
                frameIndex++;

                if (ext.Children == null)
                    continue;

                foreach (RWSceneNodeBoneMetadata plug in ext.Children)
                {
                    if (plug.BoneNameID == name)
                        return frameIndex;
                }
            }
            return -1;
        }

        public List<RWSceneNode> GetDepthFirstTree()
        {
            List<RWSceneNode> list = new List<RWSceneNode>(_struct.SceneNodeCount);
            int nodeIndex = 1; // Index starts at 1 because the first bone is always skipped
            while (nodeIndex != _struct.SceneNodeCount)
            {
                DepthFirstTraversal(_struct.SceneNodes[nodeIndex], ref list, ref nodeIndex);
            }
            return list;
        }

        public List<RWSceneNode> GetBreadthFirstTree()
        {
            List<RWSceneNode> newTree = new List<RWSceneNode>(_struct.SceneNodeCount);
            List<RWSceneNode> rootBones = _struct.SceneNodes.FindAll(frame => frame.Parent == null).ToList();
            BreadthFirstTraversal(ref newTree, rootBones);
            return newTree;
        }

        // Protected methods
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            for (int i = 0; i < _struct.SceneNodeCount; i++)
                _extensionNodes[i].InternalWrite(writer);
        }

        // Private methods
        private static void DepthFirstTraversal(RWSceneNode frame, ref List<RWSceneNode> list, ref int nIndex)
        {
            list.Add(frame);
            nIndex++;
            if (frame.Children == null)
                return;
            for (int i = 0; i < frame.Children.Count; i++)
            {
                DepthFirstTraversal(frame.Children[i], ref list, ref nIndex);
            }
        }

        private static void BreadthFirstTraversal(ref List<RWSceneNode> newTree, List<RWSceneNode> children)
        {
            foreach (RWSceneNode child in children)
            {
                newTree.Add(child);
            }

            foreach (RWSceneNode child in children)
            {
                BreadthFirstTraversal(ref newTree, child.Children);
            }
        }

        private RWSceneNode GetRoot()
        {
            foreach (RWSceneNode node in _struct.SceneNodes)
            {
                if (node.HasBoneMetadata && node.BoneMetadata.IsRootBone)
                    return node;
            }

            return null;
        }
    }
}
