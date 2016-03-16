using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWSceneNodeListStruct : RWNode
    {
        // Fields
        private List<RWSceneNode> _sceneNodes;

        // Properties
        public int SceneNodeCount
        {
            get { return _sceneNodes.Count; }
        }

        public List<RWSceneNode> SceneNodes
        {
            get { return _sceneNodes; }
        }

        // Constructors
        public RWSceneNodeListStruct(IList<RWSceneNode> frames)
            : base(RWNodeType.Struct)
        {
            _sceneNodes = frames.ToList();
        }

        internal RWSceneNodeListStruct(RWNodeFactory.RWNodeInfo header, BinaryReader reader)
                : base(header)
        {
            int frameCount = reader.ReadInt32();
            _sceneNodes = new List<RWSceneNode>(frameCount);

            for (int i = 0; i < frameCount; i++)
            {
                _sceneNodes.Add(new RWSceneNode(reader, _sceneNodes));
            }
        }

        // Methods
        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(SceneNodeCount);
            for (int i = 0; i < SceneNodeCount; i++)
                _sceneNodes[i].InternalWrite(writer, _sceneNodes);
        }
    }
}