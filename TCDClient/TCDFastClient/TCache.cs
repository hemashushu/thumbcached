using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections;
using Doms.TCClient.Configuration;

namespace Doms.TCClient
{
    /// <summary>
    /// ThumbCached client
    /// </summary>
    public class TCache
    {
        public static readonly DateTime NoItemTime = DateTime.MinValue;
        public static readonly DateTime NoSpecifyModifiedTime = DateTime.MinValue;

        private string _serverId;

        private static TCClientConfigSection _config = TCClientConfigSection.Instance;

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
            Proxy.Add(
                getServerEndPoint(_serverId),
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

            return Proxy.Get(
                getServerEndPoint(_serverId),
                key, ifModifiedSince);
        }
        #endregion 

        #region multi-get
        public TCacheItem[] MultiGet(string[] keys)
        {
            return Proxy.MultiGet(
                getServerEndPoint(_serverId),
                keys);
        }
        #endregion

        #region remove
        /// <summary>
        /// Remove a cache item
        /// </summary>
        public void Remove(string key)
        {
            Proxy.Remove(
                getServerEndPoint(_serverId),
                key);
        }
        #endregion

        #region server status
        public string GetServerStatus()
        {
            return Proxy.GetServerStatus(
                getServerEndPoint(_serverId));
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

        #region private functions
        private string getServerEndPoint(string serverid)
        {
            //get server address by server id
            ServerEndpointConfigElement node = _config.ServerNodes.GetServerNode(serverid);
            if (node == null)
            {
                throw new InvalidOperationException("The specify server not found");
            }
            return node.ToString();
        }
        #endregion
    }
}

//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com