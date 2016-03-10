namespace AtlusLibSharp.Graphics.MT
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    public struct ABKeyInfo
    {
        public const int SIZE = 8;

        [FieldOffset(0)]
        public ABKeyType KeyType;

        [FieldOffset(2)]
        public short UnkMorph;
        
        [FieldOffset(4)]
        public int BoneIndex;
    }
}
