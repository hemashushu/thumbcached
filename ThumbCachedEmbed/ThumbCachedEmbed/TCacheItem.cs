using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Embed
{
    public class TCacheItem
    {
        //item key
        private string _key;

        //item content last modified time
        private DateTime _itemTime;

        //the binary content
        private object _value;

        internal TCacheItem(string key, object value, DateTime itemTime)
        {
            _key = key;
            _value = value;
            _itemTime = itemTime;
        }

        public string Key
        {
            get { return _key; }
        }

        public DateTime ItemTime
        {
            get { return _itemTime; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}
