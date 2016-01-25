using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWTextureDictionary : RWNode
    {
        private RWTextureDictionaryStruct _struct;
        private RWTextureNative[] _textures;
        private RWExtension _extension;

        public RWTextureDictionaryStruct Struct
        {
            get { return _struct; }
            set
            {
                _struct = value;
                _struct.Parent = this;
            }
        }

        public RWTextureNative[] Textures
        {
            get { return _textures; }
            set
            {
                _textures = value;
                for (int i = 0; i < _textures.Length; i++)
                {
                    if (_textures[i] != null)
                        _textures[i].Parent = this;
                }
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

        internal RWTextureDictionary(uint size, uint version, RWNode parent, BinaryReader reader)
            : base(RWType.TextureDictionary, size, version, parent)
        {
            Struct = ReadNode(reader, this) as RWTextureDictionaryStruct;
            Textures = new RWTextureNative[Struct.TextureCount];
            for (int i = 0; i < Struct.TextureCount; i++)
            {
                Textures[i] = ReadNode(reader, this) as RWTextureNative;
            }
            Extension = ReadNode(reader, this) as RWExtension;
        }

        public RWTextureDictionary(IEnumerable<RWTextureNative> textures)
            : base(RWType.TextureDictionary)
        {
            Textures = textures.ToArray();
            Extension = new RWExtension { Plugins = new List<RWNode>() };
            Struct = new RWTextureDictionaryStruct(this);
        }

        protected override void InternalWriteData(BinaryWriter writer)
        {
            _struct.Write(writer);
            for (int i = 0; i < _struct.TextureCount; i++)
            {
                _textures[i].Write(writer);
            }
            _extension.Write(writer);
        }
    }
}
