using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmicitiaLibrary.Utilities
{
    public static class EncodingCache
    {
        public static Encoding ShiftJIS { get; } = Encoding.GetEncoding( 932 );
    }
}
