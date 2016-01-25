using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWFrameList : RWNode
    {
        // Fields
        private RWFrameListStruct _struct;
        private List<RWExtension> _extension;

        // Properties
        public RWFrameListStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                if (value == null)
                    return;
                _struct.Parent = this;
            }
        }

        public List<RWExtension> Extensions
        {
            get { return _extension; }
            set
            {
                _extension = value;
                for (int i = 0; i < _extension.Count; i++)
                {
                    _extension[i].Parent = this;
                }
            }
        }

        // Constructors
        public RWFrameList(RWFrameListStruct str, List<RWExtension> extensions)
            : base(RWType.FrameList)
        {
            Struct = str;
            Extensions = extensions;
        }

        internal RWFrameList(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
                : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWFrameListStruct>(this, reader);
            _extension = new List<RWExtension>(_struct.FrameCount);
            for (int i = 0; i < Struct.FrameCount; i++)
                _extension.Add(RWNodeFactory.GetNode<RWExtension>(this, reader));
        }

        // Public methods
        public RWFrame GetFrameByHierarchyIndex(uint hierarchyIndex)
        {
            return _struct.Frames[(int)(HierarchyIndexToFrameIndex(hierarchyIndex))];
        }

        public int GetFrameIndexByNameID(uint nameID)
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

        public int GetHierarchyIndexByNameID(uint nameID)
        {
            RWHierarchyAnimPlugin root = GetRoot(this);
            foreach (RWHierarchyAnimNode node in root.NodeList)
            {
                if (node.FrameNameID == nameID)
                    return (int)node.HierarchyIndex;
            }
            return -1;
        }

        public int HierarchyIndexToFrameIndex(uint hierarchyIndex)
        {
            int name = -1;

            RWHierarchyAnimPlugin root = GetRoot(this);
            foreach (RWHierarchyAnimNode node in root.NodeList)
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
            List<RWFrame> rootBones = Array.FindAll(_struct.Frames, (frame => frame.ParentIndex == -1)).ToList();
            BreadthFirstTraversal(ref newTree, rootBones);
            return newTree;
        }

        public RWHierarchyAnimPlugin GetRoot()
        {
            foreach (RWExtension ext in _extension)
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

        // Protected methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            for (int i = 0; i < _struct.FrameCount; i++)
                _extension[i].InternalWrite(writer);
        }

        // Static methods
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
    }
}
