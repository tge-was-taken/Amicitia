namespace AtlusLibSharp.Tables.Persona4
{
    using System.IO;

    // TODO
    internal struct ModelAnimationProperties
    {
        private ushort _unknown1;
        private ushort _animationSpeed;
        private ushort _unknown2;
        private ushort _animationSpeed2;
        private ushort _unknown3;

        internal ModelAnimationProperties(BinaryReader reader)
        {
            _unknown1 = reader.ReadUInt16();
            _animationSpeed = reader.ReadUInt16();
            _unknown2 = reader.ReadUInt16();
            _animationSpeed2 = reader.ReadUInt16();
            _unknown3 = reader.ReadUInt16();
        }

        internal void InternalWrite(BinaryWriter writer)
        {
            writer.Write(_unknown1);
            writer.Write(_animationSpeed);
            writer.Write(_unknown2);
            writer.Write(_animationSpeed2);
            writer.Write(_unknown3);
        }
    }
}
