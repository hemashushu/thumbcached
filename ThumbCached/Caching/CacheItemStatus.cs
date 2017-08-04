using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching
{
    /// <summary>
    /// The status of cache item
    /// </summary>
    class CacheItemStatus
    {
        //flag to indicate whether the cache item has stored or not
        private bool _hasStored;

        //the last access time
        private DateTime _lastAccessTime;

        //expiration time, and it's absolute time or relative time
        //NOTE:: in persistance mode, these two value cannot specify by client, 
        //  it's the server business
        // _expirationSecond == 0 meaning no expiration
        private int _expirationSecond;
        private bool _absoluteExpire;

        //item hits, i.e. how many times this cache item has been visited
        private int _hits;

        //item key
        private string _itemKey;

        //cache item
        private CacheItem _cacheItem;

        public CacheItemStatus(
            CacheItem item,
            bool hasStored, DateTime nowTime, int expirationSecond, bool absExpire)
        {
            _cacheItem = item;
            
            _hits = 0;
            _hasStored = hasStored;
            _lastAccessTime = nowTime;
            _expirationSecond = expirationSecond;
            _absoluteExpire = absExpire;
            _itemKey = item.Key;
        }

        public string ItemKey
        { 
            get { return _itemKey; } 
        }

        public CacheItem Item
        {
            get { return _cacheItem; }
            set { _cacheItem = value; }
        }

        public bool HasStored 
        {
            get { return _hasStored; }
            set { _hasStored = value; }
        }

        public int Hits
        {
            get { return _hits; }
            set { _hits = value; }
        }

        public DateTime LastAccessTime
        {
            get { return _lastAccessTime; }
            set { _lastAccessTime = value; }
        }

        public int ExpirationSecond
        {
            get { return _expirationSecond; }
            set { _expirationSecond = value; }
        }

        public bool AbsoluteExpire
        {
            get { return _absoluteExpire; }
            set { _absoluteExpire = value; }
        }


    }
}
