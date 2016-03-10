#if USE_OPENTK_EXTENSIONS
namespace GameIO
{
    using System;
    using OpenTK;

    public static class GameIOReaderOpenTKExtensions
    {
        // Half extension

        public static Half ReadHalf(this GameIOReader reader)
        {
            return Half.FromBytes(BitConverter.GetBytes(reader.ReadInt16()), 0);
        }

        // Vector extensions

        public static Vector2 ReadVector2(this GameIOReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector2h ReadVector2h(this GameIOReader reader)
        {
            return new Vector2h(reader.ReadHalf(), reader.ReadHalf());
        }

        public static Vector3 ReadVector3(this GameIOReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3h ReadVector3h(this GameIOReader reader)
        {
            return new Vector3h(reader.ReadHalf(), reader.ReadHalf(), reader.ReadHalf());
        }

        public static Vector4 ReadVector4(this GameIOReader reader)
        {
            return new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector4h ReadVector4h(this GameIOReader reader)
        {
            return new Vector4h(reader.ReadHalf(), reader.ReadHalf(), reader.ReadHalf(), reader.ReadHalf());
        }

        // Quaternion extension

        public static Quaternion ReadQuaternion(this GameIOReader reader)
        {
            return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        // Matrix extensions

        public static Matrix2 ReadMatrix2(this GameIOReader reader)
        {
            return new Matrix2(reader.ReadVector2(), reader.ReadVector2());
        }

        public static Matrix2x3 ReadMatrix2x3(this GameIOReader reader)
        {
            return new Matrix2x3(reader.ReadVector3(), reader.ReadVector3());
        }

        public static Matrix2x4 ReadMatrix2x4(this GameIOReader reader)
        {
            return new Matrix2x4(reader.ReadVector4(), reader.ReadVector4());
        }

        public static Matrix3 ReadMatrix3(this GameIOReader reader)
        {
            return new Matrix3(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
        }

        public static Matrix3x2 ReadMatrix3x2(this GameIOReader reader)
        {
            return new Matrix3x2(reader.ReadVector2(), reader.ReadVector2(), reader.ReadVector2());
        }

        public static Matrix3x4 ReadMatrix3x4(this GameIOReader reader)
        {
            return new Matrix3x4(reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4());
        }

        public static Matrix4 ReadMatrix4(this GameIOReader reader)
        {
            return new Matrix4(reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4(), reader.ReadVector4());
        }

        public static Matrix4x2 ReadMatrix4x2(this GameIOReader reader)
        {
            return new Matrix4x2(reader.ReadVector2(), reader.ReadVector2(), reader.ReadVector2(), reader.ReadVector2());
        }

        public static Matrix4x3 ReadMatrix4x3(this GameIOReader reader)
        {
            return new Matrix4x3(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
        }
    }
}
#endif
