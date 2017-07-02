namespace AtlusLibSharp.Graphics.MT
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Size = SIZE)]
    public struct AbKeyInfo
    {
        public const int SIZE = 8;

        [FieldOffset(0)]
        public AbKeyType KeyType;

        [FieldOffset(2)]
        public short UnkMorph;
        
        [FieldOffset(4)]
        public int BoneIndex;
    }
}
