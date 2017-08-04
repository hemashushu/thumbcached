using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Doms.TCClient
{
    /// <summary>
    /// Non-persistent ThumbCached client
    /// </summary>
    public class TCacheNP
    {
        private static readonly DateTime NoItemTime = DateTime.MinValue;
        public static readonly TimeSpan NoExpiration = TimeSpan.Zero;
        public static readonly DateTime NoAbsoluteExpiration = DateTime.MaxValue;

        #region add or update
        public static void Add(string key, object val)
        {
            Add(key, val, TCacheNP.NoExpiration);
        }

        /// <summary>
        /// Add or update a cache item
        /// </summary>
        public static void Add(string key, object val, TimeSpan expirationTime)
        {
            Proxy.Add(
                NonPersistentNodesList.GetServerEndPoint(key),
                new TCacheItem(key, val, TCacheNP.NoItemTime),
                (int)expirationTime.TotalSeconds, false);
        }

        public static void Add(string key, object val, DateTime absExpirationTime)
        {
            int expirationSecond = 0;
            if (absExpirationTime > DateTime.MinValue)
            {
                if (absExpirationTime < DateTime.Now)
                {
                    return;
                }
                
                double totalSeconds = (absExpirationTime - DateTime.Now).TotalSeconds;
                if(totalSeconds>int.MaxValue)
                {
                    expirationSecond= int.MaxValue;
                }
                else
                {
                    expirationSecond = (int)totalSeconds;
                }
            }
            Proxy.Add(
                NonPersistentNodesList.GetServerEndPoint(key),
                new TCacheItem(key, val, TCacheNP.NoItemTime),
                expirationSecond, true);
        }

        #endregion

        #region get
        public static object Get(string key)
        {
            TCacheItem item = GetItem(key);
            if (item == null)
            {
                return null;
            }
            else
            {
                return item.Value;
            }
        }

        /// <summary>
        /// Get a cache item
        /// </summary>
        public static TCacheItem GetItem(string key)
        {
            //NOTE::
            //If the specify block does not exist, this method will return NULL

            try
            {
                return Proxy.Get(
                    NonPersistentNodesList.GetServerEndPoint(key),
                    key, DateTime.MinValue);
            }
            catch (CacheItemNotFoundException)
            {
                return null;
            }
        }
        #endregion 

        #region multi-get
        public static TCacheItem[] MultiGet(string[] keys)
        {
            //divide keys into groups
            List<NodeGroup> groups = new List<NodeGroup>();
            foreach(string key in keys)
            {
                string endPoint = NonPersistentNodesList.GetServerEndPoint(key);

                NodeGroup existGroup = null;
                foreach(NodeGroup  group in groups)
                {
                    if(group.EndPoint == endPoint)
                    {
                        existGroup = group;
                        break;
                    }
                }

                if (existGroup == null)
                {
                    NodeGroup newGroup = new NodeGroup(endPoint);
                    newGroup.AddKey(key);
                    groups.Add(newGroup);
                }
                else
                {
                    existGroup.AddKey(key);
                }
            }

            //get items
            List<TCacheItem> items = new List<TCacheItem>(keys.Length);
            foreach (NodeGroup group in groups)
            {
                TCacheItem[] ci = Proxy.MultiGet(group.EndPoint, group.Keys);
                items.AddRange(ci);
            }

            return items.ToArray();
        }
        #endregion

        #region remove
        /// <summary>
        /// Remove a cache item
        /// </summary>
        public static void Remove(string key)
        {
            Proxy.Remove(
                NonPersistentNodesList.GetServerEndPoint(key),
                key);
        }
        #endregion

        #region internal class
        private class NodeGroup
        {
            private List<string> _keys = new List<string>();
            private string _endPoint;

            public NodeGroup(string endPoint)
            {
                _endPoint = endPoint;
            }

            public void AddKey(string key)
            {
                _keys.Add(key);
            }

            public string EndPoint
            {
                get { return _endPoint; }
            }

            public string[] Keys
            {
                get { return _keys.ToArray(); }
            }
        }
        #endregion
    }

}
