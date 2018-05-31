namespace AmicitiaLibrary.Graphics.TGA
{ 
    /// <summary>
    /// Truevision TARGA image data encoding id enumeration. Black and white, Run-length encoding and compression are currently not implemented.
    /// </summary>
    public enum TgaEncoding : byte
    {
        /// <summary>
        /// No image data included.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Uncompressed, color-mapped images.
        /// </summary>
        Indexed = 0x01,

        /// <summary>
        ///  Uncompressed, RGB images.
        /// </summary>
        RGB = 0x02,

        /// <summary>
        /// Uncompressed, black and white images.
        /// </summary>
        Grayscale = 0x03,

        /// <summary>
        /// Runlength encoded color-mapped images.
        /// </summary>
        IndexedRLE = 0x09,

        /// <summary>
        /// Runlength encoded RGB images.
        /// </summary>
        RGBRLE = 0x0A,

        /// <summary>
        /// Compressed, black and white images.
        /// </summary>
        GrayScaleCmp = 0x0B,

        /// <summary>
        /// Compressed color-mapped data, using Huffman, Delta, and runlength encoding.
        /// </summary>
        IndexedHDRLE = 0x20,

        /// <summary>
        /// Compressed color-mapped data, using Huffman, Delta, and runlength encoding. 4-pass quadtree-id process.
        /// </summary>
        IndexedHDRLEQ = 0x21
    }
}
