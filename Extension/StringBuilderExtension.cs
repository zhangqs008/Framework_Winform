using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Whir.Framework.Extension
{
    public static class StringBuilderExtension
    {
        public static void Clear(this StringBuilder value)
        {
            value.Length = 0;
            value.Capacity = 0;
        }
    }
}
