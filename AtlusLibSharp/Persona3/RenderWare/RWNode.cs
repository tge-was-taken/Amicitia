using System;
using System.Collections.Generic;
using System.IO;

namespace AtlusLibSharp.Persona3.RenderWare
{
    public class RWNode : BinaryFileBase
    {
        public const uint Persona3Version = 0x1C020037;
        protected static uint DefaultVersion = Persona3Version;

        private static uint RW_HEADER_SIZE = 12;
        private RWType _type;
        private uint _size;
        private uint _rawVersion;
        private RWNode _parent;
        private List<RWNode> _children;
        private byte[] _data;

        public RWType Type
        {
            get { return _type; }
        }

        public uint Size
        {
            get { return _size; }
        }

        public float Version
        {
            get
            {
                return 3 + ((float)((_rawVersion & 0xFFFF0000) >> 16) / 10000);
            }
        }

        public uint Revision
        {
            get
            {
                return (_rawVersion & 0xFFFF);
            }

            set
            {
                _rawVersion = (_rawVersion & 0xFFFF0000) | value & 0xFFFF;
            }
        }

        public RWNode Parent
        {
            get { return _parent; }
            internal set
            {
                if (value == null) return;
                if (value._children == null)
                    value._children = new List<RWNode>();
                if (!value._children.Contains(this))
                    value._children.Add(this);
                _parent = value;
            }
        }

        public List<RWNode> Children
        {
            get { return _children; }
            protected set { _children = value; }
        }

        public RWNode(RWType type)
        {
            _type = type;
            _size = 0;
            _rawVersion = DefaultVersion;
            _parent = null;
        }

        internal RWNode(RWNodeFactory.RWNodeProcHeader header)
        {
            _type = header.Type;
            _size = header.Size;
            _rawVersion = header.Version;
            _parent = header.Parent;
        }

        internal RWNode(RWNodeFactory.RWNodeProcHeader header, BinaryReader reader)
        {
            _type = header.Type;
            _size = header.Size;
            _rawVersion = header.Version;
            _parent = header.Parent;
            _data = reader.ReadBytes((int)Size);
        }

        internal override void InternalWrite(BinaryWriter writer)
        {
            long headerPosition = writer.BaseStream.Position;
            writer.BaseStream.Position += RW_HEADER_SIZE;

            InternalWriteData(writer);

            // Calculate size of this node
            long endPosition = writer.BaseStream.Position;
            _size = (uint)(endPosition - (headerPosition + RW_HEADER_SIZE));

            // Seek back to where the header should be, and write it using the calculated size.
            writer.BaseStream.Position = headerPosition;
            writer.Write((uint)Type);
            writer.Write(Size);
            writer.Write(DefaultVersion);

            // Seek to the end of this node
            writer.BaseStream.Position = endPosition;
        }

        protected virtual void InternalWriteData(BinaryWriter writer)
        {
            writer.Write(_data);
        }
    }
    
    public static class RWNodeFactory
    {
        internal struct RWNodeProcHeader
        {
            public RWType Type;
            public uint Size;
            public uint Version;
            public RWNode Parent;
        }

        internal static T GetNode<T>(RWNode parent, BinaryReader reader)
            where T : RWNode
        {
            return (T)GetNode(parent, reader);
        }

        internal static RWNode GetNode(RWNode parent, BinaryReader reader)
        {
            RWNodeProcHeader header = new RWNodeProcHeader
            {
                Type = (RWType)reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Version = reader.ReadUInt32(),
                Parent = parent
            };

            switch (header.Type)
            {
                case RWType.Struct:
                    return GetStructNode(header, reader);

                case RWType.FrameList:
                    return new RWFrameList(header, reader);

                case RWType.Clump:
                    return new RWClump(header, reader);

                case RWType.Extension:
                    return new RWExtension(header, reader);

                case RWType.GeometryList:
                    return new RWGeometryList(header, reader);

                case RWType.Geometry:
                    return new RWGeometry(header, reader);

                case RWType.Atomic:
                    return new RWAtomic(header, reader);

                case RWType.Material:
                    return new RWMaterial(header, reader);

                case RWType.MaterialList:
                    return new RWMaterialList(header, reader);

                case RWType.String:
                    return new RWString(header, reader);

                case RWType.TextureReference:
                    return new RWTextureReference(header, reader);

                case RWType.SkinPlugin:
                    return new RWSkinPlugin(header, reader, parent.Parent as RWGeometry);

                case RWType.HierarchyAnimPlugin:
                    return new RWHierarchyAnimPlugin(header, reader);

                case RWType.TextureDictionary:
                    return new RWTextureDictionary(header, reader);

                case RWType.TextureNative:
                    return new RWTextureNative(header, reader);

                default:
                    return new RWNode(header, reader);
            }
        }

        private static RWNode GetStructNode(RWNodeProcHeader header, BinaryReader reader)
        {
            switch (header.Parent.Type)
            {
                case RWType.Clump:
                    return new RWClumpStruct(header, reader);

                case RWType.FrameList:
                    return new RWFrameListStruct(header, reader);

                case RWType.GeometryList:
                    return new RWGeometryListStruct(header, reader);

                case RWType.Geometry:
                    return new RWGeometryStruct(header, reader);

                case RWType.Material:
                    return new RWMaterialStruct(header, reader);

                case RWType.MaterialList:
                    return new RWMaterialListStruct(header, reader);

                case RWType.TextureReference:
                    return new RWTextureReferenceStruct(header, reader);

                case RWType.Atomic:
                    return new RWAtomicStruct(header, reader);

                case RWType.TextureDictionary:
                    return new RWTextureDictionaryStruct(header, reader);

                case RWType.TextureNative:
                    {
                        RWTextureNative txn = header.Parent as RWTextureNative;

                        if (txn.Struct == null)
                            return new RWTextureNativeStruct(header, reader);

                        else if (txn.Raster == null)
                            return new RWRaster(header, reader);

                        else throw new NotImplementedException();
                    }

                case RWType.Struct:
                    {
                        if (header.Parent.Parent == null)
                            return new RWNode(header, reader);

                        switch (header.Parent.Parent.Type)
                        {
                            case RWType.TextureNative:
                                {
                                    RWRaster raster = header.Parent as RWRaster;

                                    if (raster.Info == null)
                                        return new RWRasterInfo(header, reader);

                                    else if (raster.Data == null)
                                        return new RWRasterData(header, reader);

                                    else throw new NotImplementedException();
                                }
                            default:
                                throw new NotImplementedException();
                        }
                    }

                default:
                    return new RWNode(header, reader);

            }
        }
    }
}
