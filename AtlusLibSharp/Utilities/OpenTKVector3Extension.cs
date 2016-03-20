using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Utilities
{
    public static class OpenTKVector3Extension
    {
        public static bool IsNaN(this OpenTK.Vector3 value)
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

        public static Assimp.Vector3D ToAssimpVector3D(this OpenTK.Vector3 value)
        {
            return new Assimp.Vector3D(value.X, value.Y, value.Z);
        }
    }

    public static class OpenTKVector2Extension
    {
        public static Assimp.Vector3D ToAssimpVector3D(this OpenTK.Vector2 value, float z)
        {
            return new Assimp.Vector3D(value.X, value.Y, z);
        }
    }
}
