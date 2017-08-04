using System;
using System.Collections.Generic;
using System.Text;
using Doms.ThumbCached.Caching;

namespace Doms.ThumbCached.Embed
{
    class Proxy
    {
        #region singleton
        private static Proxy _instance = new Proxy();

        public static Proxy Instance
        {
            get { return _instance; }
        }
        #endregion

        #region init and dispose
        private bool _disposed;
        private CacheManagerCollection _cacheMans;

        private Proxy()
        {
            _disposed = false;
            _cacheMans = new CacheManagerCollection();
        }

        ~Proxy()
        {
            if (!_disposed)
            {
                _disposed = true;
                _cacheMans.Close();
            }
        }
        #endregion

        #region operations
        public void Add(string nodeId, TCacheItem item, int expirationSecond, bool absExpire)
        {
            CacheManager man = _cacheMans.GetCacheManager(nodeId);

            //serialize data
            byte[] data;
            CacheDataType dataType;
            DataSerialize.Serialize(item.Value, out data, out dataType);

            CacheItem ci = new CacheItem(item.Key, data, item.ItemTime, (int)dataType);
            man.Set(ci, expirationSecond, absExpire);
        }

        public TCacheItem Get(string nodeId, string key, DateTime ifModifySince)
        {
            CacheManager man = _cacheMans.GetCacheManager(nodeId);

            CacheItem ci = null;
            try
            {
                ci=man.Get(key);
            }
            catch (ItemNotFoundException)
            {
                throw new CacheItemNotFoundException();
            }

            if (ifModifySince != DateTime.MinValue)
            {
                long span = (long)(ci.ItemTime - ifModifySince).TotalSeconds;
                if (span <= 0)
                {
                    //Content not modify since the specify time
                    throw new CacheItemNotModifiedException(ci.ItemTime);
                }
            }

            //deserialize data
            object val = DataSerialize.Deserialize(ci.Content, (CacheDataType)ci.Properties);
            TCacheItem item = new TCacheItem(ci.Key, val, ci.ItemTime);
            return item;
        }

        public TCacheItem[] MultiGet(string nodeId, string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException("keys");
            }

            CacheManager man = _cacheMans.GetCacheManager(nodeId);
            List<TCacheItem> items = new List<TCacheItem>();

            foreach (string key in keys)
            {
                try
                {
                    CacheItem ci = man.Get(key);

                    //deserialize data
                    object val = DataSerialize.Deserialize(ci.Content, (CacheDataType)ci.Properties);
                    TCacheItem item = new TCacheItem(ci.Key, val, ci.ItemTime);
                    items.Add(item);
                }
                catch (ItemNotFoundException)
                {
                    //ignore not found item(s)
                }
            }

            return items.ToArray();
        }

        public void Remove(string nodeId, string key)
        {
            CacheManager man = _cacheMans.GetCacheManager(nodeId);
            man.Remove(key);
        }
        #endregion

    }
}
