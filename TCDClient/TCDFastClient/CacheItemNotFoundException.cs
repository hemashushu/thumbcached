using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.TCClient
{
    public class CacheItemNotFoundException:Exception
    {
        public CacheItemNotFoundException() : base() { }

        public override string Message
        {
            get
            {
                return "Cache item not found";
            }
        }
    }
}
