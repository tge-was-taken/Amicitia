namespace AtlusLibSharp.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public static class MatrixExtension
    {
        private static Type _matrix4x4Type = typeof(Matrix4x4);

        private static Dictionary<int, Func<Matrix4x4, float>> _getElementFuncs = new Dictionary<int, Func<Matrix4x4, float>>()
        {
            { 0 | 0 << 2, (mtx) => mtx.M11 }, { 1 | 0 << 2, (mtx) => mtx.M21 }, { 2 | 0 << 2, (mtx) => mtx.M31 }, { 3 | 0 << 2, (mtx) => mtx.M41 },
            { 0 | 1 << 2, (mtx) => mtx.M12 }, { 1 | 1 << 2, (mtx) => mtx.M22 }, { 2 | 1 << 2, (mtx) => mtx.M32 }, { 3 | 1 << 2, (mtx) => mtx.M42 },
            { 0 | 2 << 2, (mtx) => mtx.M13 }, { 1 | 2 << 2, (mtx) => mtx.M23 }, { 2 | 2 << 2, (mtx) => mtx.M33 }, { 3 | 2 << 2, (mtx) => mtx.M43 },
            { 0 | 3 << 2, (mtx) => mtx.M14 }, { 1 | 3 << 2, (mtx) => mtx.M24 }, { 2 | 3 << 2, (mtx) => mtx.M34 }, { 3 | 3 << 2, (mtx) => mtx.M44 },
        };

        private static Dictionary<int, Action<Matrix4x4, float>> _setElementFuncs = new Dictionary<int, Action<Matrix4x4, float>>()
        {
            { 0 | 0 << 2, (mtx, val) => mtx.M11 = val }, { 1 | 0 << 2, (mtx, val) => mtx.M21 = val }, { 2 | 0 << 2, (mtx, val) => mtx.M31 = val }, { 3 | 0 << 2, (mtx, val) => mtx.M41 = val },
            { 0 | 1 << 2, (mtx, val) => mtx.M12 = val }, { 1 | 1 << 2, (mtx, val) => mtx.M22 = val }, { 2 | 1 << 2, (mtx, val) => mtx.M32 = val }, { 3 | 1 << 2, (mtx, val) => mtx.M42 = val },
            { 0 | 2 << 2, (mtx, val) => mtx.M13 = val }, { 1 | 2 << 2, (mtx, val) => mtx.M23 = val }, { 2 | 2 << 2, (mtx, val) => mtx.M33 = val }, { 3 | 2 << 2, (mtx, val) => mtx.M43 = val },
            { 0 | 3 << 2, (mtx, val) => mtx.M14 = val }, { 1 | 3 << 2, (mtx, val) => mtx.M24 = val }, { 2 | 3 << 2, (mtx, val) => mtx.M34 = val }, { 3 | 3 << 2, (mtx, val) => mtx.M44 = val },
        };

        public static float GetElementAt(this Matrix4x4 mtx, int rowIndex, int elementIndex)
        {
            return _getElementFuncs[rowIndex | elementIndex << 2].Invoke(mtx);
        }

        public static void SetElementAt(this Matrix4x4 mtx, int rowIndex, int elementIndex, float value)
        {
            string fieldname = $"M{rowIndex + 1}{elementIndex + 1}";
            var field = _matrix4x4Type.GetField(fieldname);
            field.SetValue(mtx, value);
            //setElementFuncs[rowIndex | elementIndex << 2].Invoke(mtx, value);
        }

        public static Assimp.Matrix4x4 ToAssimpMatrix4x4(this Matrix4x4 mtx)
        {
            Assimp.Matrix4x4 outMtx = new Assimp.Matrix4x4();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    outMtx[i+1, j+1] = mtx.GetElementAt(i, j);
                }
            }
            return outMtx;
        }
    }
}
