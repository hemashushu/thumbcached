using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching.Storing
{
    public class StoringException:Exception
    {
        public StoringException() : base() { }
        public StoringException(string message) : base(message) { }
    }
}
