using System;
using System.Runtime.InteropServices;

namespace NvTriStrip
{
    public enum PrimType
    {
        PT_LIST,
        PT_STRIP,
        PT_FAN
    }

    public unsafe struct PrimitiveGroup
    {
        public PrimType type;
        public uint numIndices;
        public ushort* indices;
    }

    /// <summary>
    /// Utility class for calling methods in the Nvidia TriStrip library. Offers mesh index buffer and vertex cache optimization.
    /// </summary>
    public unsafe static class NvTriStrip
    {
        /// <summary>
        /// Generate triangle strips from the given triangle indices.
        /// </summary>
        /// <param name="triangles">The array containing the triangle indices.</param>
        /// <returns>Tri-stripped triangle indices</returns>
        public static ushort[] GenerateStrips(ushort[] triangles)
        {
            fixed (ushort* p = &triangles[0])
            {
                PrimitiveGroup* primGroup = null;
                ushort numPrimGroups = 0;
                bool success = GenerateStrips(p, (uint)triangles.Length, &primGroup, &numPrimGroups);

                if (!success)
                {
                    throw new Exception("Failed to generate triangle strips!");
                }

                try
                {
                    if (numPrimGroups != 1)
                    {
                        throw new Exception("More than 1 primitive group was returned");
                    }

                    if (primGroup[0].type != PrimType.PT_STRIP)
                    {
                        throw new Exception("Returned primitive group isn't triangle stripped!");
                    }

                    ushort[] strippedIndices = new ushort[primGroup[0].numIndices];

                    for (int i = 0; i < strippedIndices.Length; ++i)
                        strippedIndices[i] = primGroup[0].indices[i];

                    return strippedIndices;
                }
                finally
                {
                    DeletePrimitiveGroups(primGroup, numPrimGroups);
                }
            }
        }

        // Credit to Tom Nuydens (tom@delphi3d.net) for the dll wrapper

        /// <summary>
        /// Sets the cache size which the stripfier uses to optimize the data.
        /// Controls the length of the generated individual strips.
        /// This is the "actual" cache size, so 24 for GeForce3 and 16 for GeForce1/2
        /// You may want to play around with this number to tweak performance.
        ///
        /// Default value: 16
        /// </summary>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsSetCacheSize@4")]
        public extern static void SetCacheSize(uint cacheSize);

        /// <summary>
        /// bool to indicate whether to stitch together strips into one huge strip or not.
        /// If set to true, you'll get back one huge strip stitched together using degenerate
        ///  triangles.
        /// If set to false, you'll get back a large number of separate strips.
        ///
        /// Default value: true
        /// </summary>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsSetStitchStrips@4")]
        public extern static void SetStitchStrips(bool bStitchStrips);

        /// <summary>
        /// Sets the minimum acceptable size for a strip, in triangles.
        /// All strips generated which are shorter than this will be thrown into one big, separate list.
        ///
        /// Default value: 0
        /// </summary>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsSetMinStripLength@4")]
        public extern static void SetMinStripSize(uint minStripSize);

        /// <summary>
        /// If set to true, will return an optimized list, with no strips at all.
        ///
        /// Default value: false
        ///
        /// </summary>
        /// <param name="_bListsOnly"></param>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsSetListOnly@4")]
        public extern static void SetListsOnly(bool bListsOnly);

        /// <summary>
        /// in_indices: input index list, the indices you would use to render
        /// in_numIndices: number of entries in in_indices
        /// primGroups: array of optimized/stripified PrimitiveGroups
        /// numGroups: number of groups returned
        ///
        /// Be sure to call delete[] on the returned primGroups to avoid leaking mem
        ///
        /// </summary>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsGenerateStrips@16")]
        public extern static bool GenerateStrips(ushort* in_indices, uint in_numIndices, PrimitiveGroup** primGroups, ushort* numGroups);

        /// <summary>
        /// Function to remap your indices to improve spatial locality in your vertex buffer.
        ///
        /// in_primGroups: array of PrimitiveGroups you want remapped
        /// numGroups: number of entries in in_primGroups
        /// numVerts: number of vertices in your vertex buffer, also can be thought of as the range
        ///  of acceptable values for indices in your primitive groups.
        /// remappedGroups: array of remapped PrimitiveGroups
        ///
        /// Note that, according to the remapping handed back to you, you must reorder your 
        ///  vertex buffer.
        ///
        /// Credit goes to the MS Xbox crew for the idea for this interface.
        ///
        /// </summary>
        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsRemapIndices@16")]
        public extern static void RemapIndices(PrimitiveGroup* in_primGroups, ushort numGroups, ushort numVerts, PrimitiveGroup** remappedGroups);

        [DllImport("NvTriStrip.dll", EntryPoint = "_nvtsDeletePrimitiveGroups@8")]
        public extern static void DeletePrimitiveGroups(PrimitiveGroup* primGroups, ushort numGroups);
    }
}