using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Embed
{
    public class CacheItemNotFoundException : Exception
    {
        public CacheItemNotFoundException() : base() { }
        public CacheItemNotFoundException(string message) : base(message) { }
    }
}
