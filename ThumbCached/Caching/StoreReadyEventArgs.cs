using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching
{
    /// <summary>
    /// Args for "StoreReady" Event 
    /// </summary>
    class StoreReadyEventArgs : EventArgs
    {
        private string _itemKey;

        public string ItemKey
        {
            get { return _itemKey; }
        }

        public StoreReadyEventArgs(string itemKey)
        {
            _itemKey = itemKey;
        }
    }
}
