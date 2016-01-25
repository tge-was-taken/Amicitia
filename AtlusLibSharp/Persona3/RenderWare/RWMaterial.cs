using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWMaterial : RWNode
    {
        private RWMaterialStruct _struct;
        private RWTextureReference _textureReference;
        private RWExtension _extension;

        public RWMaterialStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public RWTextureReference TextureReference
        {
            get { return _textureReference; }
            set
            {
                _textureReference = value;
                _textureReference.Parent = this;
            }
        }

        public RWExtension Extension
        {
            get { return _extension; }
            set
            {
                _extension = value;
                _extension.Parent = this;
            }
        }

        internal RWMaterial(uint size, uint version, RWNode parent, BinaryReader reader)
                : base(RWType.Material, size, version, parent)
        {
            Struct = ReadNode(reader, this) as RWMaterialStruct;
            if (Struct.IsTextured)
                TextureReference = ReadNode(reader, this) as RWTextureReference;
            Extension = ReadNode(reader, this) as RWExtension;
        }

        public RWMaterial(string textureName = null)
            : base(RWType.Material)
        {
            Struct = new RWMaterialStruct(this, (textureName != null));
            if (textureName != null)
                TextureReference = new RWTextureReference(textureName);
            Extension = new RWExtension() { Plugins = new List<RWNode>() };
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            Struct.Write(writer);
            if (Struct.IsTextured)
                TextureReference.Write(writer);
            Extension.Write(writer);
        }
    }
}