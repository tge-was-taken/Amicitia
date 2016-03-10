using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    public class RWFrameList : RWNode
    {
        // Fields
        private RWFrameListStruct _struct;
        private List<RWExtension> _extensionNodes;
        private RWHierarchyAnimPlugin _root;

        public RWHierarchyAnimPlugin HierarchyAnimRoot
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
        public List<RWFrame> Frames
        {
            get { return _struct.Frames; }
        }

        public int FrameCount
        {
            get { return _struct.FrameCount; }
        }

        internal List<RWExtension> Extensions
        {
            get { return _extensionNodes; }
        }

        // Constructors
        internal RWFrameList(IList<RWFrame> frames, List<RWExtension> extensions)
            : base(RWType.FrameList)
        {
            _struct = new RWFrameListStruct(frames);
            _extensionNodes = extensions;
        }

        internal RWFrameList(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWFrameListStruct>(this, reader);
            _extensionNodes = new List<RWExtension>(_struct.FrameCount);
            for (int i = 0; i < _struct.FrameCount; i++)
                _extensionNodes.Add(RWNodeFactory.GetNode<RWExtension>(this, reader));
        }

        // Public methods
        public RWFrame GetFrameByHierarchyIndex(int hierarchyIndex)
        {
            return _struct.Frames[ConvertHierarchyIndexToFrameIndex(hierarchyIndex)];
        }

        public int GetFrameIndexByNameID(int nameID)
        {
            int frameIdx = -1;
            foreach (RWExtension ext in Extensions)
            {
                frameIdx++;

                if (ext.Children == null || ext.Children.Count == 0)
                    continue;

                foreach (RWHierarchyAnimPlugin plug in ext.Children)
                {
                    if (plug.FrameNameID == nameID)
                        return frameIdx;
                }
            }
            return -1;
        }

        public int GetHierarchyIndexByNameID(int nameID)
        {
            RWHierarchyAnimPlugin root = GetRoot();
            foreach (RWHierarchyAnimNode node in root.Nodes)
            {
                if (node.FrameNameID == nameID)
                    return (int)node.HierarchyIndex;
            }
            return -1;
        }

        public int ConvertHierarchyIndexToFrameIndex(int hierarchyIndex)
        {
            int name = -1;

            RWHierarchyAnimPlugin root = GetRoot();
            foreach (RWHierarchyAnimNode node in root.Nodes)
            {
                if (node.HierarchyIndex == hierarchyIndex)
                    name = (int)node.FrameNameID;
            }

            if (name == -1)
                return -1;

            int frameIndex = -1;
            foreach (RWExtension ext in Extensions)
            {
                frameIndex++;

                if (ext.Children == null)
                    continue;

                foreach (RWHierarchyAnimPlugin plug in ext.Children)
                {
                    if (plug.FrameNameID == name)
                        return (int)frameIndex;
                }
            }
            return -1;
        }

        public List<RWFrame> GetDepthFirstTree()
        {
            List<RWFrame> list = new List<RWFrame>(_struct.FrameCount);
            int nodeIndex = 1; // Index starts at 1 because the first bone is always skipped
            while (nodeIndex != _struct.FrameCount)
            {
                DepthFirstTraversal(_struct.Frames[nodeIndex], ref list, ref nodeIndex);
            }
            return list;
        }

        public List<RWFrame> GetBreadthFirstTree()
        {
            List<RWFrame> newTree = new List<RWFrame>(_struct.FrameCount);
            List<RWFrame> rootBones = _struct.Frames.FindAll(frame => frame.ParentIndex == -1).ToList();
            BreadthFirstTraversal(ref newTree, rootBones);
            return newTree;
        }

        // Protected methods
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            for (int i = 0; i < _struct.FrameCount; i++)
                _extensionNodes[i].InternalWrite(writer);
        }

        // Private methods
        private static void DepthFirstTraversal(RWFrame frame, ref List<RWFrame> list, ref int nIndex)
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

        private static void BreadthFirstTraversal(ref List<RWFrame> newTree, List<RWFrame> children)
        {
            foreach (RWFrame child in children)
            {
                newTree.Add(child);
            }

            foreach (RWFrame child in children)
            {
                BreadthFirstTraversal(ref newTree, child.Children);
            }
        }

        private RWHierarchyAnimPlugin GetRoot()
        {
            foreach (RWExtension ext in _extensionNodes)
            {
                if (ext.Children == null || ext.Children.Count == 0)
                    continue;

                foreach (RWHierarchyAnimPlugin plug in ext.Children)
                {
                    if (plug.NodeCount != 0)
                        return plug;
                }
            }

            return null;
        }
    }
}
