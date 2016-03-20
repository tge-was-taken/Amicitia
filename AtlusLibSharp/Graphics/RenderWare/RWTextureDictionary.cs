namespace AtlusLibSharp.Graphics.RenderWare
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Encapsulates a RenderWare texture dictionary and all of its corresponding data structures.
    /// </summary>
    public class RWTextureDictionary : RWNode
    {
        private RWTextureDictionaryStruct _struct;
        private List<RWTextureNative> _textures;
        private RWExtension _extension;

        #region Properties

        /***************************************************/
        /* RWTextureDictionaryStruct forwarded properties */
        /***************************************************/

        /// <summary>
        /// Gets the <see cref="RWDeviceID"/> of this texture dictionary. Used to indicate for which platform the data is compiled.
        /// </summary>
        public RWDeviceID DeviceID
        {
            get
            {
                if (_struct != null)
                    return _struct.DeviceID;
                else
                    return RWDeviceID.PS2;
            }
        }

        /// <summary>
        /// Gets the amount of textures contained in this texture dictionary.
        /// </summary>
        public int TextureCount
        {
            get
            {
                if (_textures != null)
                    return _textures.Count;
                else
                    return 0;
            }
        }

        /****************************/
        /* Non-forwarded properties */
        /****************************/

        /// <summary>
        /// Gets the list of texture nodes contained in this texture dictionary.
        /// </summary>
        public List<RWTextureNative> Textures
        {
            get { return _textures; }
            set
            {
                _textures = value;

                if (_textures == null)
                    return;

                for (int i = 0; i < _textures.Count; i++)
                {
                    if (_textures[i] != null)
                        _textures[i].Parent = this;
                }
            }
        }

        /// <summary>
        /// Gets the list of extension nodes applied to this texture dictionary.
        /// </summary>
        public List<RWNode> ExtensionNodes
        {
            get
            {
                if (_extension != null)
                    return _extension.Children;
                else
                    return null;
            }
        }

        #endregion

        /// <summary>
        /// Initialize a new <see cref="RWTextureDictionary"/> instance with an <see cref="IList{T}"/> of texture nodes.
        /// </summary>
        /// <param name="textures"><see cref="IList{T}"/>containing texture nodes to initialize the dictionary with.</param> 
        public RWTextureDictionary(IList<RWTextureNative> textures)
            : base(RWNodeType.TextureDictionary)
        {
            Textures = textures.ToList();
            _extension = new RWExtension(this);
            _struct = new RWTextureDictionaryStruct(this);
        }

        /// <summary>
        /// Initialize a new empty <see cref="RWTextureDictionary"/> instance.
        /// </summary>
        public RWTextureDictionary()
            : base(RWNodeType.TextureDictionary)
        {
            _struct = new RWTextureDictionaryStruct(this);
            _textures = new List<RWTextureNative>();
            _extension = new RWExtension(this);
        }

        /// <summary>
        /// Constructor only to be called in <see cref="RWNodeFactory.GetNode(RWNode, BinaryReader)"/>.
        /// </summary>
        internal RWTextureDictionary(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
            : base(header)
        {
            _struct = RWNodeFactory.GetNode<RWTextureDictionaryStruct>(this, reader);
            _textures = new List<RWTextureNative>(_struct.TextureCount);

            for (int i = 0; i < _struct.TextureCount; i++)
            {
                _textures.Add(RWNodeFactory.GetNode<RWTextureNative>(this, reader));
            }

            _extension = RWNodeFactory.GetNode<RWExtension>(this, reader);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // Update the texture count in the struct
            _struct.TextureCount = (ushort)TextureCount;

            // And write the updated struct to the stream
            _struct.InternalWrite(writer);

            // Write textures
            foreach (RWTextureNative texture in _textures)
            {
                texture.InternalWrite(writer);
            }

            // Write extension
            _extension.InternalWrite(writer);
        }
    }
}
