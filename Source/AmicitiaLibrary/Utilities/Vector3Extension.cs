using System.Numerics;

namespace AmicitiaLibrary.Utilities
{
    public static class Vector3Extension
    {
        public static bool IsNaN(this Vector3 value)
        {
            if (float.IsNaN(value.X))
                return true;
            else if (float.IsNaN(value.Y))
                return true;
            else if (float.IsNaN(value.Z))
                return true;
            else
                return false;
        }

        public static Assimp.Vector3D ToAssimpVector3D(this Vector3 value)
        {
            return new Assimp.Vector3D(value.X, value.Y, value.Z);
        }
    }

    public static class Vector2Extension
    {
        public static Assimp.Vector3D ToAssimpVector3D(this Vector2 value, float z)
        {
            return new Assimp.Vector3D(value.X, value.Y, z);
        }
    }
}
