#if USE_DRAWING_EXTENSIONS
namespace GameIO
{
    using System.Drawing;

    public static class GameIOReaderDrawingExtensions
    {
        public static Color ReadRGBA8888Color(this GameIOReader reader)
        {
            return Color.FromArgb(reader.ReadInt32());
        }

        public static Color ReadRGB888Color(this GameIOReader reader)
        {
            return Color.FromArgb(reader.ReadInt32() | 0xFF); // set the alpha to 255
        }

        public static Color ReadRGBA5551Color(this GameIOReader reader)
        {
            ushort val = reader.ReadUInt16();

            return Color.FromArgb(
                (val & 1) == 1 ? 255 : 0,
                ((val & 0x1F) << 00) << 3,
                ((val & 0x1F) << 05) << 3,
                ((val & 0x1F) << 10) << 3
            );
        }
    }
}
#endif
