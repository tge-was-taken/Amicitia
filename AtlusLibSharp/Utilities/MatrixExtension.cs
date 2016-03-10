namespace AtlusLibSharp.Utilities
{
    public static class MatrixExtension
    {
        public static OpenTK.Matrix4 ToMatrix4(this OpenTK.Matrix4x3 mtx)
        {
            OpenTK.Matrix4 outMtx = OpenTK.Matrix4.Identity;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    outMtx[i, j] = mtx[i, j];
                }
            }
            return outMtx;
        }

        public static Assimp.Matrix4x4 ToAssimpMatrix4x4(this OpenTK.Matrix4 mtx)
        {
            Assimp.Matrix4x4 outMtx = Assimp.Matrix4x4.Identity;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    outMtx[i+1, j+1] = mtx[i, j];
                }
            }
            return outMtx;
        }

        public static OpenTK.Matrix4x3 ToMatrix4x3(this OpenTK.Matrix4 mtx)
        {
            OpenTK.Matrix4x3 outMtx = OpenTK.Matrix4x3.Zero;
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
