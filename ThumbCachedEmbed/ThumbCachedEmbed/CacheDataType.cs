using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Embed
{
    enum CacheDataType
    {
        Binary = 0,
        Object = 1,
        Boolean = 3,
        Int32 = 9,
        Int64 = 11,
        Single = 13,
        Double = 14,
        DateTime = 16,
        String = 18,

        EmptyByteArray = 256 + 6,
        EmptyString = 256 + 18
    }
}
