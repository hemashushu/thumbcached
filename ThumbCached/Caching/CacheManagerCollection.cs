using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using Doms.ThumbCached.Configuration;

namespace Doms.ThumbCached.Caching
{
    /// <summary>
    /// PoolManager collection
    /// </summary>
    public class CacheManagerCollection
    {
        //private ListDictionary _pools;
        private List<string> _nodeIds;
        private List<CacheManager> _managers;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public CacheManagerCollection()
        {
            //_pools = new ListDictionary(StringComparer.CurrentCultureIgnoreCase);
            _nodeIds = new List<string>();
            _managers = new List<CacheManager>();

            foreach (CacheNodeConfigElement node in
                CachingConfigSection.Instance.CacheNodes)
            {
                _nodeIds.Add(node.NodeId);
                _managers.Add(new CacheManager(node));
            }
        }

        /// <summary>
        /// Close all PoolManager
        /// </summary>
        public void Close()
        {

            logger.Info("Stoping cache serivce, wait for storageing...");

            foreach (CacheManager pool in _managers)
            {
                pool.Close();
            }
        }

        public List<CacheManager> Managers
        {
            get { return _managers; }
        }

        public CacheManager GetCacheManager(string bindEndPointName)
        {
            int pos = _nodeIds.IndexOf(bindEndPointName);
            if (pos >= 0)
            {
                return _managers[pos];
            }
            else
            {
                throw new InvalidOperationException("The specify node id not found");
            }
        }

    }
}


//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com