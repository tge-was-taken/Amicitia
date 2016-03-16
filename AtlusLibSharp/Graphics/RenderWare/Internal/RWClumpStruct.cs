using System;
using System.IO;

namespace AtlusLibSharp.Graphics.RenderWare
{
    internal class RWSceneStruct : RWNode
    {
        private int _drawCallCount;
        private int _lightCount;
        private int _cameraCount;

        public int DrawCallCount
        {
            get { return _drawCallCount; }
        }

        public int LightCount
        {
            get { return _lightCount; }
        }

        public int CameraCount
        {
            get { return _cameraCount; }
        }

        internal RWSceneStruct(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
            : base(header)
        {
            _drawCallCount = reader.ReadInt32();
            _lightCount = reader.ReadInt32(); 
            _cameraCount = reader.ReadInt32();
        }

        internal RWSceneStruct(RWScene scene)
            : base(new RWNodeFactory.RWNodeProcHeader { Parent = scene, Type = RWNodeType.Struct, Version = ExportVersion })
        {
            _drawCallCount = scene.DrawCalls.Count;
            _lightCount = 0;
            _cameraCount = 0;
        }

        protected internal override void InternalWriteInnerData(BinaryWriter writer)
        {
            writer.Write(_drawCallCount);
            writer.Write(_lightCount);
            writer.Write(_cameraCount);
        }
    }
}
