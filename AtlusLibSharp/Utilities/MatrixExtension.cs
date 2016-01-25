using OpenTK;

namespace AtlusLibSharp.Utilities
{
    public static class MatrixExtension
    {
        public static Matrix4 ToMatrix4(this Matrix4x3 mtx)
        {
            Matrix4 outMtx = Matrix4.Identity;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    outMtx[i, j] = mtx[i, j];
                }
            }
            return outMtx;
        }
    }
}
