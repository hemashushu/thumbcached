using System;
using System.Collections.Generic;
using System.Text;
using Doms.ThumbCached.Caching.Storing;
using Doms.ThumbCached.Configuration;

namespace Doms.ThumbCached.Caching
{
    /// <summary>
    /// Cache pool manager
    /// </summary>
    public class CacheManager
    {
        #region private variables
        //cache items
        private Dictionary<string, CacheItemStatus> _cacheStatus;

        //synchronize object for pool collections
        private object _syncObject;

        //indicate stop accpeting new cache, and program is going to shutdown
        private bool _stopping;

        //configuration values
        private readonly int _poolMemorySize; //the size of pool (in bytes)
        private readonly string _nodeId; //associate with binding endpoint name
        private readonly bool _persistEnable; //enable persistance or not

        //the used size of pool currently
        private int _poolUsedSize = 0;

        //the pool reserve size (in bytes), 
        //while cleaning up, the pool should remove old/inactive items to recover memory
        //until the free memory reach to this size
        private readonly int _poolReserveSize;

        //the storing manager
        private StoringManager _storeManager;

        //the storing queue, new and updated cache item will send to this queue and wait for storing
        private StoringQueue _storeQueue;

        //the persistance item keep alive time
        private const int _persistanceItemKeepAliveTime = 10 * 60;

        //the now time
        private DateTime _nowTime;

        private readonly DateTime _baseTime = new DateTime(2000, 1, 1);

        //the timer that update now time
        private System.Timers.Timer _secondTimer;

        //the timer that clean expired items
        private System.Timers.Timer _cleanTimer;

        //do cleaning interval
        private int _cleaningInterval = 10 * 60 * 1000;

        //logger
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region initial
        public CacheManager(CacheNodeConfigElement config)
        {
            _syncObject = new object();

            //collections
            _cacheStatus = new Dictionary<string, CacheItemStatus>(100000);

            //read config
            _poolMemorySize = config.PoolSize * 1024 * 1024;
            _nodeId = config.NodeId;
            _persistEnable = config.PersistEnable;

            _poolReserveSize = _poolMemorySize / 20; //default reserve size is 5% of pool size

            if (_persistEnable && config.Storing == null)
            {
                throw new InvalidOperationException("Storing config not found");
            }

            //storing manager
            _storeManager = new StoringManager(
                config.Storing.IndexFile,
                config.Storing.DataFile,
                _persistEnable);

            //storing queue
            _storeQueue = new StoringQueue();
            _storeQueue.StoreReady += new EventHandler<StoreReadyEventArgs>(storeQueue_StoreReady);
            _storeQueue.Start();

            //the timer that update now time
            _secondTimer = new System.Timers.Timer();
            _secondTimer.Interval = 1000; //update interval
            _secondTimer.Elapsed += new System.Timers.ElapsedEventHandler(secondTimer_Elapsed);
            _secondTimer.Start();

            //the timer that clean expired items
            _cleanTimer = new System.Timers.Timer();
            _cleanTimer.Interval = _cleaningInterval; //clean interval
            _cleanTimer.Elapsed += new System.Timers.ElapsedEventHandler(cleanTimer_Elapsed);
            _cleanTimer.Start();
        }

        #endregion

        #region properties
        /// <summary>
        /// Pool memory size
        /// </summary>
        public int MemorySize
        {
            get { return _poolMemorySize; }
        }

        /// <summary>
        /// The used memory size
        /// </summary>
        public int UsedMemorySize
        {
            get { return _poolUsedSize; }
        }

        /// <summary>
        /// Calculate total hits
        /// </summary>
        public long CacheTotalHits
        {
            get
            {
                long hits = 0;
                lock (_syncObject)
                {
                    foreach (CacheItemStatus status in _cacheStatus.Values)
                    {
                        hits += status.Hits;
                    }
                }
                return hits;
            }
        }

        /// <summary>
        /// The amount of cache items
        /// </summary>
        public int CacheCount
        {
            get { return _cacheStatus.Count; }
        }

        /// <summary>
        /// Current node id
        /// </summary>
        public string NodeId
        {
            get { return _nodeId; }
        }
        #endregion

        #region Set (add or update)
        /// <summary>
        /// Add one cache item
        /// </summary>
        public void Set(CacheItem item, int expirationSecond, bool absExpire)
        {
            if (_stopping)
                throw new InvalidOperationException("Cache pool has closed");

            if (item.Content == null || item.Content.Length == 0)
            {
                throw new ArgumentNullException("The content is empty");
            }

            //conver date time
            if (item.ItemTime == DateTime.MinValue)
            {
                item.ItemTime = _nowTime;
            }

            lock (_syncObject)
            {
                if (_persistEnable)
                    setItemWithPersistance(item, expirationSecond, absExpire);
                else
                    setItem(item, expirationSecond, absExpire);
            }
        }

        private void setItem(CacheItem item, int expirationSecond, bool absExpire)
        {
            //clean inactive items
            if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
            {
                cleanUpItems(true);
            }

            if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
            {
                logger.Warn("Node '{0}' out of memory",_nodeId);
                throw new ApplicationException("Out of memory");
            }

            //try fetch from cache pool first
            CacheItemStatus existItem;
            if (_cacheStatus.TryGetValue(item.Key, out existItem))
            {
                #region update cache item
                logger.Debug("Update the cache '{0}' in cache pool", item.Key);

                _poolUsedSize += (item.Content.Length - existItem.Item.Content.Length);

                //update the cache in cache pool
                existItem.Item = item;

                //update status
                existItem.ExpirationSecond = expirationSecond;
                existItem.AbsoluteExpire = absExpire;
                existItem.LastAccessTime = _nowTime;
                existItem.Hits = 0;
                #endregion
            }
            else
            {
                #region add new cache item
                logger.Debug("Add new cache '{0}' to cache pool", item.Key);

                CacheItemStatus staus = new CacheItemStatus(
                    item, true, _nowTime, expirationSecond, absExpire);

                //add to cache pool
                _cacheStatus.Add(item.Key, staus);

                _poolUsedSize += item.Content.Length;
                #endregion
            }
        }

        private void setItemWithPersistance(CacheItem item, int expirationSecond, bool absExpire)
        {
            //clean inactive items
            if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
            {
                cleanUpItems(true);
            }

            if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
            {
                //store directly
                _storeManager.Store(item);
                return;
            }

            //try fetch from cache pool first
            CacheItemStatus existItem;
            if (_cacheStatus.TryGetValue(item.Key, out existItem))
            {
                #region update cache item
                logger.Debug("Update the cache '{0}' in cache pool", item.Key);

                _poolUsedSize += (item.Content.Length - existItem.Item.Content.Length);

                //update the cache in cache pool
                existItem.Item = item;

                //update status //ignore the client specify expiration time
                //existItem.ExpirationSecond = expirationSecond;
                //existItem.AbsoluteExpire = absExpire;
                existItem.LastAccessTime = _nowTime;
                existItem.Hits = 0;

                //reset this flag
                existItem.HasStored = false;

                //add item to storing queue
                _storeQueue.AddItem(item.Key);
                #endregion
            }
            else
            {
                #region add new cache item
                logger.Debug("Add new cache '{0}' to cache pool", item.Key);

                //create new status object, ignore the client specify expiration time
                CacheItemStatus staus = new CacheItemStatus(
                    item, false, _nowTime, _persistanceItemKeepAliveTime, false); //expirationSecond, absExpire);

                //add to cache pool
                _cacheStatus.Add(item.Key, staus);

                _poolUsedSize += item.Content.Length;

                //add item to storing queue
                _storeQueue.AddItem(item.Key);
                #endregion
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Fetch the specify item
        /// </summary>
        public CacheItem Get(string key)
        {
            //NOTE::
            // BlockNotFoundException will be throwed if the specify cache doesn't exist,

            if (_stopping)
                throw new InvalidOperationException("Cache pool has closed");

            lock (_syncObject)
            {
                if (_persistEnable)
                    return getItemWithPersistance(key);
                else
                    return getItem(key);
            }
        }

        private CacheItem getItem(string key)
        {
            //try fetch from cache pool first
            CacheItemStatus existItem;
            if (_cacheStatus.TryGetValue(key, out existItem))
            {
                #region fetch from cache pool
                logger.Debug("Fetch cache item '{0}' from cache pool", key);

                //check if expired
                if (existItem.ExpirationSecond > 0 &&
                    (_nowTime - existItem.LastAccessTime).TotalSeconds >= existItem.ExpirationSecond)
                {
                    //remove this cache item from pool
                    _cacheStatus.Remove(key);
                    GC.WaitForPendingFinalizers();

                    _poolUsedSize -= existItem.Item.Content.Length;

                    throw new ItemNotFoundException("Cache not found");
                }

                //update status
                if (!existItem.AbsoluteExpire)
                {
                    existItem.LastAccessTime = _nowTime;
                }

                if (existItem.Hits < int.MaxValue)
                {
                    existItem.Hits++;
                }

                return new CacheItem(
                    key, existItem.Item.Content,
                    existItem.Item.ItemTime, existItem.Item.Properties);

                #endregion
            }
            else
            {
                throw new ItemNotFoundException("Cache not found");
            }
        }

        private CacheItem getItemWithPersistance(string key)
        {
            //try fetch from cache pool first
            CacheItemStatus existItem;
            if (_cacheStatus.TryGetValue(key, out existItem))
            {
                #region fetch from cache pool
                logger.Debug("Fetch cache item '{0}' from cache pool", key);

                //update status
                if (!existItem.AbsoluteExpire)
                {
                    existItem.LastAccessTime = _nowTime;
                }

                if (existItem.Hits < int.MaxValue)
                {
                    existItem.Hits++;
                }

                return new CacheItem(
                    key, existItem.Item.Content,
                    existItem.Item.ItemTime, existItem.Item.Properties);

                #endregion
            }
            else
            {
                #region fetch from storing
                logger.Debug("Fetch cache item '{0}' from storing manager", key);

                //fetch from storing
                try
                {
                    CacheItem item = _storeManager.Fetch(key);

                    if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
                    {
                        cleanUpItems(true);
                    }

                    if (_poolUsedSize + item.Content.Length >= _poolMemorySize)
                    {
                        //return cache item directly
                        return item;
                    }

                    //insert item into cache pool
                    CacheItemStatus status = new CacheItemStatus(
                        item, true, _nowTime, _persistanceItemKeepAliveTime, false);
                    _cacheStatus.Add(key, status);

                    //add used size
                    _poolUsedSize += item.Content.Length;

                    return item;
                }
                catch (StoringException)
                {
                    throw new ItemNotFoundException("Cache not found");
                }
                #endregion

            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Remove item
        /// </summary>
        public void Remove(string key)
        {
            if (_stopping)
                throw new InvalidOperationException("Cache pool has closed");

            logger.Debug("Remove cache item '{0}'", key);

            lock (_syncObject)
            {
                //remove item from cache pool
                if (_cacheStatus.ContainsKey(key))
                {
                    _poolUsedSize -= _cacheStatus[key].Item.Content.Length;
                    _cacheStatus.Remove(key);

                    GC.WaitForPendingFinalizers();
                }

                //remove item from storing manager
                if (_persistEnable) _storeManager.Remove(key);
            }
        }
        #endregion

        #region store cache item
        /// <summary>
        /// Store cache item
        /// </summary>
        private void storeQueue_StoreReady(object sender, StoreReadyEventArgs e)
        {
            CacheItem cache = null;

            lock (_syncObject)
            {
                if (_cacheStatus.ContainsKey(e.ItemKey))
                {
                    CacheItemStatus item = _cacheStatus[e.ItemKey];
                    if (item.HasStored == false)
                    {
                        item.HasStored = true; //mark it as stored
                        cache = item.Item;
                    }
                }
            }

            if (cache != null)
            {
                try
                {
                    logger.Debug("Store the cache item '{0}'", cache.Key);
                    _storeManager.Store(cache);
                }
                catch (Exception ex)
                {
                    logger.Error("Error ocurred while storing cache: {0}", ex.Message);
                }
            }
        }

        #endregion

        #region clean up the old and expired items
        /// <summary>
        /// Clean up the Inactive and expired items
        /// </summary>
        private void cleanUpItems(bool recoverReserveSpace)
        {
            if (_cacheStatus.Count == 0) return;

            lock (_syncObject)
            {
                //calculate the status score list
                //NOTE:: the lower score item remove first

                List<StatusItemScore> scoreList = new List<StatusItemScore>(_cacheStatus.Count);
                foreach (CacheItemStatus status in _cacheStatus.Values)
                {
                    if (status.HasStored)
                    {
                        if (status.ExpirationSecond > 0 &&
                            (_nowTime - status.LastAccessTime).TotalSeconds > status.ExpirationSecond)
                        {
                            scoreList.Add(new StatusItemScore(status.ItemKey, 0, status.Item.Content.Length));
                        }
                        else if (recoverReserveSpace)
                        {
                            int score = (int)(status.LastAccessTime - _baseTime).TotalSeconds;
                            scoreList.Add(new StatusItemScore(status.ItemKey, score, status.Item.Content.Length));
                        }
                    }
                }

                List<StatusItemScore> needRemoveItems = new List<StatusItemScore>();

                if (recoverReserveSpace)
                {
                    //calculate the clean up size this time
                    int needCleanUpSize = _poolReserveSize + _poolUsedSize - _poolMemorySize;
                    if (needCleanUpSize < 0) needCleanUpSize = 0;
                    scoreList.Sort(_sortScore); //sort

                    //add the low score cache items to list
                    for (int idx = 0; idx < scoreList.Count; idx++)
                    {
                        StatusItemScore item = scoreList[idx];
                        if (item.Score == 0 || needCleanUpSize > 0)
                        {
                            needRemoveItems.Add(item);
                            needCleanUpSize -= item.ContentLength;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    needRemoveItems = scoreList;
                }

                if (needRemoveItems.Count > 0)
                {
                    foreach (StatusItemScore item in needRemoveItems)
                    {
                        _poolUsedSize -= item.ContentLength;
                        _cacheStatus.Remove(item.Key);
                    }

                    GC.Collect();

                    logger.Info("Found {0} items need to clean up, After clean up, used memory size: {1}",
                        needRemoveItems.Count, this.UsedMemorySize);
                }
            }

        }

        private Comparison<StatusItemScore> _sortScore = delegate(StatusItemScore x, StatusItemScore y)
        {
            return x.Score.CompareTo(y.Score);
        };

        #endregion

        #region close
        public void Close()
        {
            // Stop the program and close pool
            _stopping = true;
            _storeQueue.CloseAndWaitForComplete();
            _storeManager.Close();
        }
        #endregion

        #region private functions
        private void secondTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //update now time
            _nowTime = e.SignalTime;
        }

        void cleanTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //clean the expired items
            cleanUpItems(false);
        }
        #endregion

    }

    #region private struct
    /// <summary>
    /// For sort cache status items
    /// </summary>
    struct StatusItemScore
    {
        public string Key;
        public int Score;
        public int ContentLength;

        public StatusItemScore(string key, int score, int contentLength)
        {
            Key = key;
            Score = score;
            ContentLength = contentLength;
        }
    }
    #endregion

}

//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com