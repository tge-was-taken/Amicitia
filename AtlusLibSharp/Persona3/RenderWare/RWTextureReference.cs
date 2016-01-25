using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWTextureReference : RWNode
    {
        // Fields
        private RWTextureReferenceStruct _struct;
        private RWString _name;
        private RWString _maskName;
        private RWExtension _extension;

        // Properties
        public RWTextureReferenceStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public string Name
        {
            get { return _name.Value; }
            set
            {
                _name = new RWString(value);
                _name.Parent = this;
            }
        }

        public string MaskName
        {
            get { return _maskName.Value; }
            set
            {
                _maskName = new RWString(value);
                _maskName.Parent = this;
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

        // Constructors
        public RWTextureReference(string textureName)
            : base(RWType.TextureReference)
        {
            Struct = new RWTextureReferenceStruct();
            Name = textureName;
            MaskName = "";
            Extension = new RWExtension { Plugins = new List<RWNode>() };
        }

        internal RWTextureReference(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWTextureReferenceStruct>(this, reader);
            _name = RWNodeFactory.GetNode<RWString>(this, reader);
            _maskName = RWNodeFactory.GetNode<RWString>(this, reader);
            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        // Methods
        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.InternalWrite(writer);
            _name.InternalWrite(writer);
            _maskName.InternalWrite(writer);
            _extension.InternalWrite(writer);
        }
    }
}