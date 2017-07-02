using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMine;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace AtlusLibSharp.Utilities
{
    public static class ColorHelper
    {
        public const double SIMILARITY_THRESHOLD_STRICT = 0.005;

        public static bool IsSimilar(this Color @this, Color other, double threshold = SIMILARITY_THRESHOLD_STRICT )
        {
            /*
            int rDifference = @this.R - other.R;
            int bDifference = @this.B - other.B;
            int gDifference = @this.G - other.G;

            float rDifferencePercent = ( float )rDifference / 255;
            float gDifferencePercent = ( float )gDifference / 255;
            float bDifferencePercent = ( float )bDifference / 255;

            float difference = ((rDifferencePercent + gDifferencePercent + bDifferencePercent) / 3) * 100;
            */

            var thisRgb = new Rgb
            {
                R = ((double) @this.R / 255),
                G = ((double) @this.G / 255),
                B = ((double) @this.B / 255)
            };

            var otherRgb = new Rgb()
            {
                R = ( ( double )other.R / 255 ),
                G = ( ( double )other.G / 255 ),
                B = ( ( double )other.B / 255 )
            };

            double deltaE = thisRgb.Compare( otherRgb, new Cie1976Comparison() );

            return ( deltaE >= -threshold && deltaE <= threshold);
        }
    }
}
