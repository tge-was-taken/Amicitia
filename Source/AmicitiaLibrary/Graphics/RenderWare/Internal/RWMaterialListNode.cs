using System.Collections;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;

    public class RwMaterialListNode : RwNode, IList<RwMaterial>
    {
        private RwMaterialListStructNode mStructNode;
        private List<RwMaterial> mMaterials;

        public RwMaterialListNode(IList<string> textureNames)
            : base(RwNodeId.RwMaterialListNode)
        {
            mMaterials = new List<RwMaterial>(textureNames.Count);

            for (int i = 0; i < textureNames.Count; i++)
            {
                mMaterials[i] = new RwMaterial(textureNames[i], this);
            }

            mStructNode = new RwMaterialListStructNode(this);
        }

        internal RwMaterialListNode(RwNodeFactory.RwNodeHeader header, BinaryReader reader)
            : base(header)
        {
            mStructNode = RwNodeFactory.GetNode<RwMaterialListStructNode>(this, reader);
            mMaterials = new List<RwMaterial>(mStructNode.MaterialCount);

            for (int i = 0; i < mStructNode.MaterialCount; i++)
            {
                mMaterials.Add(RwNodeFactory.GetNode<RwMaterial>(this, reader));
            }
        }

        public RwMaterialListNode(RwNode parent, Assimp.Material material)
            : base(RwNodeId.RwMaterialListNode, parent)
        {
            mMaterials = new List<RwMaterial>();

            string textureName = Path.GetFileNameWithoutExtension(material.TextureDiffuse.FilePath);
            if (textureName == null)
            {
                textureName = Path.GetFileNameWithoutExtension(material.Name);
                if (textureName != null)
                    mMaterials.Add(new RwMaterial(textureName, this));
                else
                    mMaterials.Add(new RwMaterial(this));
            }
            else
            {
                mMaterials.Add(new RwMaterial(Path.GetFileNameWithoutExtension(material.TextureDiffuse.FilePath), this));
            }
            
            mStructNode = new RwMaterialListStructNode(this);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            mStructNode.Write(writer);

            for (int i = 0; i < mMaterials.Count; i++)
            {
                mMaterials[i].Write(writer);
            }
        }

        // IList<RwMaterial> implementation
        public IEnumerator<RwMaterial> GetEnumerator()
        {
            return mMaterials.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mMaterials.GetEnumerator();
        }

        public void Add(RwMaterial item)
        {
            item.Parent = this;
            mMaterials.Add(item);
        }

        public void Clear()
        {
            mMaterials.Clear();
        }

        public bool Contains(RwMaterial item)
        {
            return mMaterials.Contains(item);
        }

        public void CopyTo(RwMaterial[] array, int arrayIndex)
        {
            foreach (var item in array)
            {
                item.Parent = this;
            }

            mMaterials.CopyTo(array, arrayIndex);
        }

        public bool Remove(RwMaterial item)
        {
            return mMaterials.Remove(item);
        }

        public int Count
        {
            get { return mMaterials.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IList)mMaterials).IsReadOnly; }
        }

        public int IndexOf(RwMaterial item)
        {
            return mMaterials.IndexOf(item);
        }

        public void Insert(int index, RwMaterial item)
        {
            item.Parent = this;
            mMaterials.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            mMaterials.RemoveAt(index);
        }

        public RwMaterial this[int index]
        {
            get { return mMaterials[index]; }
            set
            {
                value.Parent = this;
                mMaterials[index] = value;
            }
        }
    }
}