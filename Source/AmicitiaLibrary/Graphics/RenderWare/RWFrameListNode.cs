using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    public class RwFrameListNode : RwNode, IList<RwFrame>
    {
        // Fields
        private RwFrameListStructNode mStruct;
        private List<RwExtensionNode> mExtensionNodes;
        private RwFrame mRoot;

        public RwFrame AnimationRootNode
        {
            get
            {
                if (mRoot == null)
                {
                    mRoot = GetRoot();
                }

                return mRoot;
            }
        }

        public List<RwExtensionNode> Extensions
        {
            get { return mExtensionNodes; }
        }

        // Constructors
        public RwFrameListNode(IList<RwFrame> frames, List<RwExtensionNode> extensions)
            : base(RwNodeId.RwFrameListNode)
        {
            mStruct = new RwFrameListStructNode(frames);
            mExtensionNodes = extensions;
        }

        internal RwFrameListNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mStruct = RwNodeFactory.GetNode<RwFrameListStructNode>(this, reader);
            mExtensionNodes = new List<RwExtensionNode>(Count);

            for (int i = 0; i < Count; i++)
            {
                var extensionNode = RwNodeFactory.GetNode<RwExtensionNode>( this, reader );           

                mStruct.FrameList[i].HAnimFrameExtensionNode = extensionNode.FindChild<RwHAnimFrameExtensionNode>( RwNodeId.RwHAnimFrameExtensionNode );

                mExtensionNodes.Add(extensionNode);
            }
        }

        public RwFrameListNode(RwNode parent) : base(RwNodeId.RwFrameListNode, parent)
        {
            mStruct = new RwFrameListStructNode((RwNode)this);
            mExtensionNodes = new List<RwExtensionNode>();
        }

        // Public methods
        public RwFrame GetFrameByHierarchyIndex(int hierarchyIndex)
        {
            return mStruct.FrameList[GetFrameIndexByHierarchyIndex(hierarchyIndex)];
        }

        public int GetFrameIndexByNameId(int nameId)
        {
            int frameIdx = -1;
            foreach (RwExtensionNode ext in Extensions)
            {
                frameIdx++;

                if (ext.Children == null || ext.Children.Count == 0)
                    continue;

                foreach (RwHAnimFrameExtensionNode plug in ext.Children)
                {
                    if (plug.NameId == nameId)
                        return frameIdx;
                }
            }
            return -1;
        }

        public int GetHierarchyIndexByNameId(int nameId)
        {
            foreach (RwHAnimNodeInfo node in AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes)
            {
                if (node.NodeId == nameId)
                    return node.Index;
            }
            return -1;
        }

        public int GetFrameIndexByHierarchyIndex(int hierarchyIndex)
        {
            int name = -1;

            foreach (RwHAnimNodeInfo node in AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes)
            {
                if (node.Index == hierarchyIndex)
                    name = node.NodeId;
            }

            if (name == -1)
                return -1;

            int frameIndex = -1;
            foreach (RwExtensionNode ext in Extensions)
            {
                frameIndex++;

                if (ext.Children == null)
                    continue;

                foreach (RwHAnimFrameExtensionNode plug in ext.Children)
                {
                    if (plug.NameId == name)
                        return frameIndex;
                }
            }
            return -1;
        }

        public int GetHierarchyIndexByFrameIndex(int frameIndex)
        {
            int name = mStruct.FrameList[frameIndex].HasHAnimExtension
                ? mStruct.FrameList[frameIndex].HAnimFrameExtensionNode.NameId
                : -1;

            if (name == -1)
                return -1;

            return AnimationRootNode.HAnimFrameExtensionNode.Hierarchy.Nodes.Find(x => x.NodeId == name).Index;
        }

        public int GetHierarchyIndexByName(string name)
        {
            if ( name.StartsWith( "Bone", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return GetHierarchyIndexByFrameIndex( ExtractFrameIndexFromName( name ) ); // Bone
            }
            else
            {
                return GetHierarchyIndexByNameId( ExtractHAnimNodeIdFromName( name ) );
            }
        }

        public int GetFrameIndexByName(string name)
        {
            if ( name.StartsWith( "Bone", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return ExtractFrameIndexFromName(name);
            }
            else
            {
                return GetFrameIndexByNameId( ExtractHAnimNodeIdFromName( name ) );
            }
        }

        public static int ExtractFrameIndexFromName(string name)
        {
            var idPart = name.Substring( 4 );
            if ( !int.TryParse( idPart, out int id ) )
            {
                throw new Exception( $"Can't parse bone id from bone name \"{name}\"" );
            }

            // base 1 to base 0
            id -= 1;

            return id;
        }

        public static int ExtractHAnimNodeIdFromName(string name)
        {
            var fixedName = ExtractDigits( name );
            if ( !int.TryParse( fixedName, out int id ) )
            {
                throw new Exception( $"Can't parse bone id from bone name \"{name}\"" );
            }

            return id;
        }

        public int GetNameIdByFrameIndex( int index )
        {
            return this[index].HAnimFrameExtensionNode.NameId;
        }

        public int GetNameIdByName( string name )
        {
            if ( name.StartsWith( "Bone", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return GetNameIdByFrameIndex( ExtractFrameIndexFromName( name ) );
            }
            else
            {
                return ExtractHAnimNodeIdFromName( name );
            }
        }

        private static string ExtractDigits(string original)
        {
            var newString = new StringBuilder();

            foreach (char c in original)
            {
                if (char.IsDigit(c))
                    newString.Append(c);
            }

            return newString.ToString();
        }

        public List<RwFrame> GetDepthFirstTree()
        {
            List<RwFrame> list = new List<RwFrame>(Count);
            int nodeIndex = 1; // Index starts at 1 because the first bone is always skipped
            while (nodeIndex != Count)
            {
                DepthFirstTraversal(mStruct.FrameList[nodeIndex], ref list, ref nodeIndex);
            }
            return list;
        }

        public List<RwFrame> GetBreadthFirstTree()
        {
            List<RwFrame> newTree = new List<RwFrame>(Count);
            List<RwFrame> rootBones = mStruct.FrameList.FindAll(frame => frame.Parent == null).ToList();
            BreadthFirstTraversal(ref newTree, rootBones);
            return newTree;
        }

        // Protected methods
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStruct.Write(writer);
            for (int i = 0; i < Count; i++)
                mExtensionNodes[i].Write(writer);
        }

        // Private methods
        private static void DepthFirstTraversal(RwFrame frame, ref List<RwFrame> list, ref int nIndex)
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

        private static void BreadthFirstTraversal(ref List<RwFrame> newTree, List<RwFrame> children)
        {
            foreach (RwFrame child in children)
            {
                newTree.Add(child);
            }

            foreach (RwFrame child in children)
            {
                BreadthFirstTraversal(ref newTree, child.Children);
            }
        }

        private RwFrame GetRoot()
        {
            foreach (RwFrame node in mStruct.FrameList)
            {
                if (node.HasHAnimExtension && node.HAnimFrameExtensionNode.IsRootBone)
                    return node;
            }

            return null;
        }

        public IEnumerator<RwFrame> GetEnumerator()
        {
            return mStruct.FrameList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) mStruct.FrameList).GetEnumerator();
        }

        public void Add(RwFrame item)
        {
            mStruct.FrameList.Add(item);
            mExtensionNodes.Add(new RwExtensionNode(this));
        }

        public void Clear()
        {
            mStruct.FrameList.Clear();
        }

        public bool Contains(RwFrame item)
        {
            return mStruct.FrameList.Contains(item);
        }

        public void CopyTo(RwFrame[] array, int arrayIndex)
        {
            mStruct.FrameList.CopyTo(array, arrayIndex);
        }

        public bool Remove(RwFrame item)
        {
            return mStruct.FrameList.Remove(item);
        }

        public int Count
        {
            get { return mStruct.FrameList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<RwFrame>) mStruct.FrameList).IsReadOnly; }
        }

        public int IndexOf(RwFrame item)
        {
            return mStruct.FrameList.IndexOf(item);
        }

        public void Insert(int index, RwFrame item)
        {
            mStruct.FrameList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mStruct.FrameList.RemoveAt(index);
        }

        public RwFrame this[int index]
        {
            get { return mStruct.FrameList[index]; }
            set { mStruct.FrameList[index] = value; }
        }
    }
}
