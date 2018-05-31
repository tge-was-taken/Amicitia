using AmicitiaLibrary.IO;

namespace AmicitiaLibrary.Graphics.RenderWare
{
    using System.IO;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a scene of RenderWare nodes along with custom nodes stored in RMD files.
    /// </summary>
    public class RmdScene : RwNode
    {
        private RwTextureDictionaryNode mTextureDictionary;
        private List<RwClumpNode> mClumps;
        private RmdNodeLinkListNode mNodeLinks;
        private List<RmdAnimation> mAnimations;
        private List<RwNode> mMiscNodes;

        /// <summary>
        /// Gets if the scene has a texture dictionary. If this is false then the texture dictionary is set to null.
        /// </summary>
        public bool HasTextureDictionary
        {
            get { return mTextureDictionary != null; }
        }

        /// <summary>
        /// Gets or sets the <see cref="RwTextureDictionaryNode"/> in the scene. Can be set to null.
        /// </summary>
        public RwTextureDictionaryNode TextureDictionary
        {
            get { return mTextureDictionary; }
            set { mTextureDictionary = value; }
        }

        /// <summary>
        /// Gets the number of <see cref="RwClumpNode"/> in the scene.
        /// </summary>
        public int ClumpCount
        {
            get { return mClumps.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RwClumpNode"/> in the scene.
        /// </summary>
        public List<RwClumpNode> Clumps
        {
            get { return mClumps; }
        }

        /// <summary>
        /// Gets the number of <see cref="RmdNodeLink"/> in the scene.
        /// </summary>
        public int NodeLinkCount
        {
            get { return mNodeLinks.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RmdNodeLink"/> used for attaching dummies to the nodes in the scene. 
        /// </summary>
        public RmdNodeLinkListNode NodeLinks
        {
            get { return mNodeLinks; }
            set { mNodeLinks = value; }
        }

        /// <summary>
        /// Gets the number of <see cref="RmdAnimation"/> in the scene.
        /// </summary>
        public int AnimationCount
        {
            get { return mAnimations.Count; }
        }

        /// <summary>
        /// Gets the list of <see cref="RmdAnimation"/> in the scene.
        /// </summary>
        public List<RmdAnimation> Animations
        {
            get { return mAnimations; }
        }

        /// <summary>
        /// Gets the list of miscellaneous RenderWare nodes (nodes who do not have a predefined place) in the scene.
        /// </summary>
        public List<RwNode> MiscNodes
        {
            get { return mMiscNodes; }
        }

        /// <summary>
        /// Initialize a new <see cref="RmdScene"/> instance with a path to an RMD file.
        /// </summary>
        /// <param name="path">Path to an RMD file.</param>
        public RmdScene(string path) 
            : this(File.OpenRead(path), true)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="RmdScene"/> instance with a <see cref="Stream"/> containing <see cref="RmdScene"/> data.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing <see cref="RmdScene"/> data.</param>
        /// <param name="leaveOpen">Option to leave the <see cref="Stream"/> open or dispose it after loading the <see cref="RmdScene"/>.</param>
        public RmdScene(Stream stream, bool leaveOpen = false)
            : this(new BinaryReader(stream, System.Text.Encoding.Default, leaveOpen))
        {
        }

        /// <summary>
        /// Initialize a new <see cref="RmdScene"/> instance with a byte array containing <see cref="RmdScene"/> data.
        /// </summary>
        /// <param name="data"></param>
        public RmdScene(byte[] data)
            : this(new MemoryStream(data), false)
        {
        }

        /// <summary>
        /// Initialize a new empty <see cref="RmdScene"/> instance.
        /// </summary>
        public RmdScene()
            : base(RwNodeId.RmdSceneNode)
        {
            InitializeMembers();
        }

        /// <summary>
        /// Initialize a new <see cref="RmdScene"/> instance with a <see cref="BinaryReader"/> reader attached to a <see cref="Stream"/> containing <see cref="RmdScene"/> data.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> attached to a <see cref="Stream"/> containing <see cref="RmdScene"/> data.</param>
        internal RmdScene(BinaryReader reader)
            : this()
        {
            using (reader)
            {
                ReadBody(reader);
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
        /// Initializes the texture dictionary and lists of the <see cref="RmdScene"/>.
        /// </summary>
        private void InitializeMembers()
        {
            mTextureDictionary = null;
            mClumps = new List<RwClumpNode>();
            mNodeLinks = new RmdNodeLinkListNode();
            mAnimations = new List<RmdAnimation>();
            mMiscNodes = new List<RwNode>();
        }

        /// <summary>
        /// Inherited from <see cref="BinaryBase"/>. Write the <see cref="RmdScene"/> to a stream using a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> used to write to the <see cref="Stream"/>.</param>
        internal override void Write(BinaryWriter writer)
        {
            WriteBody(writer);
        }

        /// <summary>
        /// Inherited from <see cref="RwNode"/>. Writes the data beyond the header.
        /// </summary>
        /// <param name="writer">The <see cref="BinaryWriter"/> to write the data with.</param>
        protected internal override void WriteBody(BinaryWriter writer)
        {
            // Create and write the animation set count node (if there are any animation sets
            if (AnimationCount > 0)
            {
                RmdAnimationCountNode animCount = new RmdAnimationCountNode((short)AnimationCount);
                animCount.Write(writer);
            }

            // Write the misc nodes first
            foreach (RwNode miscNode in mMiscNodes)
            {
                miscNode.Write(writer);
            }

            // Then the texture dictionary (if it's present)
            if (mTextureDictionary != null)
            {
                mTextureDictionary.Write(writer);
            }

            // After that the scenes
            foreach (RwClumpNode scene in mClumps)
            {
                scene.Write(writer);
            }

            // Aaaand the attach frame list (well, only if there are any entries in the list)
            if (NodeLinkCount > 0)
            {
                // Create a new frame link list and write it.
                RmdNodeLinkListNode nodeLink = new RmdNodeLinkListNode(mNodeLinks);
                nodeLink.Write(writer);
            }

            // And last but not least- the animation sets!
            foreach (RmdAnimation animationSet in mAnimations)
            {
                // don't call Write(writer) as that would include the node header (which animation sets aren't supposed to have)
                animationSet.WriteBody(writer);
            }
        }

        /// <summary>
        /// Read the <see cref="RmdScene"/> from a <see cref="Stream"/> using a <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> attached to a <see cref="Stream"/> containing <see cref="RmdScene"/> data.</param>
        protected internal override void ReadBody(BinaryReader reader)
        {
            List<RwNode> unfilteredNodes = new List<RwNode>();

            // Initial pass, read all nodes into a list and filter the animation set count, texture dictionary, scenes and attach frame list out.
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                RwNode node = RwNodeFactory.GetNode(this, reader);

                switch (node.Id)
                {
                    case RwNodeId.RmdAnimationCountNode:
                        // skip this node as its entirely redundant
                        break;

                    case RwNodeId.RwTextureDictionaryNode:
                        mTextureDictionary = (RwTextureDictionaryNode)node;
                        break;

                    case RwNodeId.RwClumpNode:
                        mClumps.Add((RwClumpNode)node);
                        break;

                    case RwNodeId.RmdNodeLinkListNode:
                        // Retrieve the list of frame links from the node and skip the node itself
                        mNodeLinks = (RmdNodeLinkListNode)node;
                        break;

                    case RwNodeId.RmdAuthor:
                        // pass through
                        break;

                    default:
                        unfilteredNodes.Add(node);
                        break;
                }
            }

            // Second pass, sort the remaining nodes into misc nodes and animation sets
            for (int i = 0; i < unfilteredNodes.Count; i++)
            {
                switch (unfilteredNodes[i].Id)
                {
                    case RwNodeId.RwAnimationNode:
                    case RwNodeId.RwUVAnimationDictionaryNode:
                    case RwNodeId.RmdAnimationPlaceholderNode:
                    case RwNodeId.RmdAnimationInstanceNode:
                    case RwNodeId.RmdAnimationTerminatorNode:
                    case RwNodeId.RmdTransformOverrideNode:
                    case RwNodeId.RmdVisibilityAnimNode:
                    case RwNodeId.RmdParticleAnimationNode:
                        // Read an animation set, this function will increment the loop iterator variable
                        mAnimations.Add(CreateAnimationSet(unfilteredNodes, ref i));
                        break;

                    default:
                        mMiscNodes.Add(unfilteredNodes[i]);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="RmdAnimation"/> from a list of nodes and an index into the list to start parsing at.
        /// </summary>
        /// <param name="nodes">List of nodes to go through.</param>
        /// <param name="startIndex">Index into the list to start parsing at.</param>
        /// <returns>A new <see cref="RmdAnimation"/>parsed from the list of nodes.</returns>
        private RmdAnimation CreateAnimationSet(List<RwNode> nodes, ref int startIndex)
        {
            List<RwNode> animNodes = new List<RwNode>();

            while (true)
            {
                // Check if the index is outside the list bounds
                if (startIndex == nodes.Count)
                {
                    break;
                }

                RwNode node = nodes[startIndex];

                // Check the if the node is a terminator and break out of the loop if it is
                if (node.Id == RwNodeId.RmdAnimationTerminatorNode)
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

            return new RmdAnimation(animNodes, this);
        }
    }
}
