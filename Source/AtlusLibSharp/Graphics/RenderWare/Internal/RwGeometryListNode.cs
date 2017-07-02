using System;
using System.Collections;

namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class RwGeometryListNode : RwNode, IList<RwGeometryNode>
    {
        private RwGeometryListStructNode mStructNode;
        private List<RwGeometryNode> mGeometryList;

        public RwGeometryListNode(RwNode parent) : base(RwNodeId.RwGeometryListNode, parent)
        {        
            mGeometryList = new List<RwGeometryNode>();
            mStructNode = new RwGeometryListStructNode( this );
        }

        public RwGeometryListNode(IList<RwGeometryNode> geoList, RwNode parent = null)
            : base(RwNodeId.RwGeometryListNode, parent)
        {
            mGeometryList = geoList.ToList();
            foreach (var geometry in mGeometryList)
                geometry.Parent = this;
        }

        internal RwGeometryListNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
                : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwGeometryListStructNode>(this, reader);
            mGeometryList = new List<RwGeometryNode>(mStructNode.GeometryCount);

            for (int i = 0; i < mStructNode.GeometryCount; i++)
            {
                mGeometryList.Add(RwNodeFactory.GetNode<RwGeometryNode>(this, reader));
            }
        }

        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.GeometryCount = Count;
            mStructNode.Write(writer);

            // Write geometries
            for (int i = 0; i < mGeometryList.Count; i++)
            { 
                mGeometryList[i].Write(writer);
            }
        }

        // IList<RwGeometryNode> implementation
        public IEnumerator<RwGeometryNode> GetEnumerator()
        {
            return mGeometryList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) mGeometryList).GetEnumerator();
        }

        public void Add(RwGeometryNode item)
        {
            item.Parent = this;
            mGeometryList.Add(item);
        }

        public void Clear()
        {
            mGeometryList.Clear();
        }

        public bool Contains(RwGeometryNode item)
        {
            return mGeometryList.Contains(item);
        }

        public void CopyTo(RwGeometryNode[] array, int arrayIndex)
        {
            foreach (var geometry in array)
                geometry.Parent = this;

            mGeometryList.CopyTo(array, arrayIndex);
        }

        public bool Remove(RwGeometryNode item)
        {
            return mGeometryList.Remove(item);
        }

        public int Count
        {
            get { return mGeometryList.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((ICollection<RwGeometryNode>) mGeometryList).IsReadOnly; }
        }

        public int IndexOf(RwGeometryNode item)
        {
            return mGeometryList.IndexOf(item);
        }

        public void Insert(int index, RwGeometryNode item)
        {
            item.Parent = this;
            mGeometryList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mGeometryList.RemoveAt(index);
        }

        public RwGeometryNode this[int index]
        {
            get { return mGeometryList[index]; }
            set
            {
                value.Parent = this;
                mGeometryList[index] = value;
            }
        }
    }
}
