using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Embed
{
    public class CacheItemNotModifiedException : Exception
    {
        private DateTime _lastModifyTime;

        public CacheItemNotModifiedException(DateTime lastModifyTime)
            : base()
        {
            _lastModifyTime = lastModifyTime;
        }

        public DateTime LastModifyTime
        {
            get { return _lastModifyTime; }
        }

        public override string Message
        {
            get
            {
                return "Cache item not modified";
            }
        }
    }
}
