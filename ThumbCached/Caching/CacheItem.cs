using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching
{
    public class CacheItem
    {
        //item key
        private string _key;

        //last modified time of cache content
        private DateTime _itemTime;

        //the binary content
        private byte[] _content;

        //the item data type and properties
        private int _properties;

        public CacheItem(string key, byte[] content, DateTime itemTime, int properties)
        {
            _key = key;
            _content = content;
            _itemTime = itemTime;
            _properties = properties;
        }

        #region member
        public string Key
        {
            get { return _key; }
        }

        public DateTime ItemTime
        {
            get { return _itemTime; }
            set { _itemTime = value; }
        }

        public byte[] Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public int Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }
        #endregion

    }
}
