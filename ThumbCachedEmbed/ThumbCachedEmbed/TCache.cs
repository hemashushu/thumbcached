using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Embed
{
    public class TCache
    {
        public static readonly DateTime NoItemTime = DateTime.MinValue;
        public static readonly DateTime NoSpecifyModifiedTime = DateTime.MinValue;

        private string _serverId;

        #region init
        public TCache(string serverId)
        {
            _serverId = serverId;
        }
        #endregion

        #region add or update
        public void Add(string key, object val)
        {
            Add(key, val, TCache.NoItemTime);
        }

        /// <summary>
        /// Add or update a cache item
        /// </summary>
        public void Add(string key, object val, DateTime itemTime)
        {
            Proxy.Instance.Add(_serverId,
                new TCacheItem(key, val, itemTime),
                0, false);
        }
        #endregion

        #region get
        public object Get(string key)
        {
            return GetItem(key, TCache.NoSpecifyModifiedTime).Value;
        }

        public object Get(string key, DateTime ifModifiedSince)
        {
            return GetItem(key, ifModifiedSince).Value;
        }

        public TCacheItem GetItem(string key)
        {
            return GetItem(key, DateTime.MinValue);
        }

        /// <summary>
        /// Get a cache item
        /// </summary>
        public TCacheItem GetItem(string key, DateTime ifModifiedSince)
        {
            //NOTE::
            //If the ifModifySince time is newer than the exist, this method will 
            //throw CacheItemNotModifiedException exception

            //If the specify block does not exist, this method will throw
            //CacheItemNotFoundException exception.

            return Proxy.Instance.Get(
                _serverId, key, ifModifiedSince);
        }
        #endregion

        #region multi-get
        public TCacheItem[] MultiGet(string[] keys)
        {
            return Proxy.Instance.MultiGet(
                _serverId, keys);
        }
        #endregion

        #region remove
        /// <summary>
        /// Remove a cache item
        /// </summary>
        public void Remove(string key)
        {
            Proxy.Instance.Remove(
                _serverId,key);
        }
        #endregion

        #region indexer
        public object this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Add(key, value);
            }
        }

        public static TCache Nodes(string serverId)
        {
            return new TCache(serverId);
        }
        #endregion

    }
}
