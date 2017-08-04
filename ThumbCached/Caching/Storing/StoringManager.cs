using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Doms.ThumbCached.Caching;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// Persistance manager
    /// </summary>
    public class StoringManager
    {
        private bool _enableStoring;

        private DataFileAccess _dataAccess;
        private IndexFileAccess _indexAccess;

        //object for synchronous
        private object _syncObject;

        public StoringManager(string indexFile, string dataFile, bool enable)
        {
            _syncObject = new object();

            _enableStoring = enable;

            if (_enableStoring)
            {
                Initializer.Check(indexFile, dataFile);
                _indexAccess = new IndexFileAccess(indexFile);
                _dataAccess = new DataFileAccess(dataFile);
            }
        }

        public void Close()
        {
            if (_enableStoring)
            {
                _dataAccess.Close();
                _indexAccess.Close();
            }
        }

        /// <summary>
        /// Store or update item
        /// </summary>
        public void Store(CacheItem item)
        {
            if (!_enableStoring)
            {
                throw new InvalidOperationException("Does not support storing");
            }

            lock (_syncObject)
            {
                try
                {
                    PersistanceItemInfo info = _indexAccess.Get(item.Key);

                    //update old record
                    _dataAccess.Update(info.DataRecordPosition, item.Content, item.Content.Length);
                    _indexAccess.Update(
                        item.Key, item.Content.Length,
                        item.ItemTime, item.Properties);
                }
                catch (StoringException)
                {
                    //and new record
                    long position = _dataAccess.Add(item.Content);
                    PersistanceItemInfo info2 = new PersistanceItemInfo()
                    {
                        Key = item.Key,
                        ContentLength = item.Content.Length,
                        DataRecordPosition = position,
                        ItemTime = item.ItemTime,
                        Properties = item.Properties
                    };
                    _indexAccess.Add(info2);
                }
            }
        }

        /// <summary>
        /// Fetch cache infomation and content
        /// </summary>
        public CacheItem Fetch(string key)
        {
            //NOTE::
            // if the specify cache doesn't exist, this procedure will throw exception
            if (!_enableStoring)
            {
                throw new InvalidOperationException("Does not support storing");
            }

            lock (_syncObject)
            {
                PersistanceItemInfo info = _indexAccess.Get(key);
                byte[] data = new byte[info.ContentLength];
                _dataAccess.Get(info.DataRecordPosition, data, data.Length);

                CacheItem item = new CacheItem(
                    key, data,info.ItemTime, info.Properties);
                return item;
            }
        }

        /// <summary>
        /// Check the specify key exist or not
        /// </summary>
        public bool Exist(string key)
        {
            if (!_enableStoring)
            {
                throw new InvalidOperationException("Does not support storing");
            }

            lock(_syncObject)
            {
                return _indexAccess.Exist(key);
            }
        }

        /// <summary>
        /// Remove the specify cache
        /// </summary>
        public void Remove(string key)
        {
            if (!_enableStoring)
            {
                throw new InvalidOperationException("Does not support storing");
            }

            lock (_syncObject)
            {
                _indexAccess.Remove(key);

                //NOTE::
                // currently only cache infomation (store in the database) can be removed, the cache
                // content that store in the binary file cann't be removed, and the space
                // it takes up cann't be recovered also.
            }
        }

    }//end class
}

//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com