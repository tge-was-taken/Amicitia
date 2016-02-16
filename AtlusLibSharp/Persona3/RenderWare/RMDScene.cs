namespace AtlusLibSharp.Persona3.RenderWare
{
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a scene of RenderWare nodes along with custom nodes stored in RMD files.
    /// </summary>
    public class RMDScene : RWNode
    {
        private RWTextureDictionary _textureDictionary;
        private List<RWClump> _clumps;
        private List<RMDFrameLink> _frameLinks;
        private List<RMDAnimationSet> _animationSets;
        private List<RWNode> _miscNodes;

        /// <summary>
        /// Gets or sets the <see cref="RWTextureDictionary"/> in the scene. Can be set to null.
        /// </summary>
        public RWTextureDictionary TextureDictionary
        {
            get { return _textureDictionary; }
            set { _textureDictionary = value; }
        }

        /// <summary>
        /// Gets the number of <see cref="RWClump"/> in the scene.
        /// </summary>
        public int ClumpCount
        {
            get { return _clumps.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RWClump"/> models in the scene.
        /// </summary>
        public List<RWClump> Clumps
        {
            get { return _clumps; }
        }

        /// <summary>
        /// Gets the number of <see cref="RMDFrameLink"/> in the scene.
        /// </summary>
        public int FrameLinkCount
        {
            get { return _frameLinks.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RMDFrameLink"/> used for attaching dummies to the frames in the scene. 
        /// </summary>
        public List<RMDFrameLink> FrameLinks
        {
            get { return _frameLinks; }
            set { _frameLinks = value; }
        }

        /// <summary>
        /// Gets the number of <see cref="RMDAnimationSet"/> in the scene.
        /// </summary>
        public int AnimationSetCount
        {
            get { return _animationSets.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RMDAnimationSet"/> in the scene.
        /// </summary>
        public List<RMDAnimationSet> AnimationSets
        {
            get { return _animationSets; }
        }

        /// <summary>
        /// Gets the list of miscellaneous RenderWare nodes (nodes who do not have a predefined place) in the scene.
        /// </summary>
        public List<RWNode> MiscNodes
        {
            get { return _miscNodes; }
        }

        /// <summary>
        /// Initialize a new <see cref="RMDScene"/> instance with a path to an RMD file.
        /// </summary>
        /// <param name="path">Path to an RMD file.</param>
        public RMDScene(string path) 
            : this(File.OpenRead(path), true)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="RMDScene"/> instance with a <see cref="Stream"/> containing <see cref="RMDScene"/> data.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing <see cref="RMDScene"/> data.</param>
        /// <param name="leaveOpen">Option to leave the <see cref="Stream"/> open or dispose it after loading the <see cref="RMDScene"/>.</param>
        public RMDScene(Stream stream, bool leaveOpen)
            : this(new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen))
        {
        }

        /// <summary>
        /// Initialize a new <see cref="RMDScene"/> instance with a byte array containing <see cref="RMDScene"/> data.
        /// </summary>
        /// <param name="data"></param>
        public RMDScene(byte[] data)
            : this(new MemoryStream(data), false)
        {
        }

        /// <summary>
        /// Initialize a new empty <see cref="RMDScene"/> instance.
        /// </summary>
        public RMDScene()
            : base(RWType.RMDScene)
        {
            InitializeMembers();
        }

        /// <summary>
        /// Initialize a new <see cref="RMDScene"/> instance with a <see cref="BinaryReader"/> reader attached to a <see cref="Stream"/> containing <see cref="RMDScene"/> data.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> attached to a <see cref="Stream"/> containing <see cref="RMDScene"/> data.</param>
        internal RMDScene(BinaryReader reader)
            : this()
        {
            using (reader)
            {
                InternalReadScene(reader);
            }
        }

        /// <summary>
        /// Clear all of the scene data.
        /// </summary>
        public void Clear()
        {
            InitializeMembers();
        }

        /// <summary>
        /// Initializes the texture dictionary and lists of the <see cref="RMDScene"/>.
        /// </summary>
        private void InitializeMembers()
        {
            _textureDictionary = null;
            _clumps = new List<RWClump>();
            _frameLinks = new List<RMDFrameLink>();
            _animationSets = new List<RMDAnimationSet>();
            _miscNodes = new List<RWNode>();
        }

        /// <summary>
        /// Inherited from <see cref="BinaryFileBase"/>. Write the <see cref="RMDScene"/> to a stream using a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the <see cref="Stream"/>.</param>
        internal override void InternalWrite(BinaryWriter writer)
        {
            InternalWriteInnerData(writer);
        }

        /// <summary>
        /// Inherited from <see cref="RWNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data to.</param>
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            // Create and write the animation set count node (if there are any animation sets
            if (AnimationSetCount > 0)
            {
                RMDAnimationSetCount animSetCount = new RMDAnimationSetCount((short)AnimationSetCount);
                animSetCount.InternalWrite(writer);
            }

            // Write the misc nodes first
            foreach (RWNode miscNode in _miscNodes)
            {
                miscNode.InternalWrite(writer);
            }

            // Then the texture dictionary (if it's present)
            if (_textureDictionary != null)
            {
                _textureDictionary.InternalWrite(writer);
            }

            // After that the clumps
            foreach (RWClump clump in _clumps)
            {
                clump.InternalWrite(writer);
            }

            // Aaaand the attach frame list (well, only if there are any entries in the list)
            if (FrameLinkCount > 0)
            {
                // Create a new frame link list and write it.
                RMDFrameLinkList frameLink = new RMDFrameLinkList(_frameLinks);
                frameLink.InternalWrite(writer);
            }

            // And last but not least- the animation sets!
            foreach (RMDAnimationSet animationSet in _animationSets)
            {
                // don't call InternalWrite(writer) as that would include the node header (which animation sets aren't supposed to have)
                animationSet.InternalWriteInnerData(writer);
            }
        }

        /// <summary>
        /// Read the <see cref="RMDScene"/> from a <see cref="Stream"/> using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> attached to a <see cref="Stream"/> containing <see cref="RMDScene"/> data.</param>
        private void InternalReadScene(BinaryReader reader)
        {
            List<RWNode> unfilteredNodes = new List<RWNode>();

            // Initial pass, read all nodes into a list and filter the animation set count, texture dictionary, clumps and attach frame list out.
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                RWNode node = RWNodeFactory.GetNode(this, reader);

                switch (node.Type)
                {
                    case RWType.RMDAnimationSetCount:
                        // skip this node as its entirely redundant
                        break;

                    case RWType.TextureDictionary:
                        _textureDictionary = (RWTextureDictionary)node;
                        break;

                    case RWType.Clump:
                        _clumps.Add((RWClump)node);
                        break;

                    case RWType.RMDFrameLinkList:
                        // Retrieve the list of frame links from the node and skip the node itself
                        _frameLinks = (node as RMDFrameLinkList).FrameLinks;
                        break;

                    default:
                        unfilteredNodes.Add(node);
                        break;
                }
            }

            // Second pass, sort the remaining nodes into misc nodes and animation sets
            for (int i = 0; i < unfilteredNodes.Count; i++)
            {
                switch (unfilteredNodes[i].Type)
                {
                    case RWType.Animation:
                    case RWType.UVAnimationDictionary:
                    case RWType.RMDAnimationSetPlaceholder:
                    case RWType.RMDAnimationSetRedirect:
                    case RWType.RMDAnimationSetTerminator:
                    case RWType.RMDTransformOverride:
                    case RWType.RMDVisibilityAnim:
                    case RWType.RMDParticleAnimation:
                        // Read an animation set, this function will increment the loop iterator variable
                        _animationSets.Add(CreateAnimationSet(unfilteredNodes, ref i));
                        break;

                    default:
                        _miscNodes.Add(unfilteredNodes[i]);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="RMDAnimationSet"/> from a list of nodes and an index into the list to start parsing at.
        /// </summary>
        /// <param name="nodes">List of nodes to go through.</param>
        /// <param name="startIndex">Index into the list to start parsing at.</param>
        /// <returns>A new <see cref="RMDAnimationSet"/>parsed from the list of nodes.</returns>
        private RMDAnimationSet CreateAnimationSet(List<RWNode> nodes, ref int startIndex)
        {
            List<RWNode> animNodes = new List<RWNode>();

            while (true)
            {
                // Check if the index is outside the list bounds
                if (startIndex == nodes.Count)
                {
                    break;
                }

                RWNode node = nodes[startIndex];

                // Check the if the node is a terminator and break out of the loop if it is
                if (node.Type == RWType.RMDAnimationSetTerminator)
                { 
                    break;
                }
                else
                {
                    // Else just increment the index and add it to the node list
                    startIndex++;
                    animNodes.Add(node);
                }
            }

            return new RMDAnimationSet(animNodes, this);
        }
    }
}
