using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Doms.ThumbCached.Caching
{
    /// <summary>
    /// Store blocks in queue
    /// </summary>
    class StoringQueue
    {
        //items that need for storing
        private Queue<string> _storingItems;

        //notify cache manager that an item can be store now
        public event EventHandler<StoreReadyEventArgs> StoreReady;

        //a flag to indicate the cache items are storing
        private bool _isStoring;

        //synchronize object for queue
        private object _syncObject;

        //scan timer
        private System.Timers.Timer _scanTimer;

        public StoringQueue()
        {
            _syncObject = new object();
            _storingItems = new Queue<string>();

            //the timer that update now time
            _scanTimer = new System.Timers.Timer();
            _scanTimer.Interval = 1000; //scan interval
            _scanTimer.Elapsed += new System.Timers.ElapsedEventHandler(scanTimer_Elapsed);
        }

        public void Start()
        {
            _scanTimer.Start();
        }

        private void scanTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_storingItems.Count > 0 && !_isStoring)
            {
                _isStoring = true; //set the flag
                doDequeue();

                //Thread thread = new Thread(new ThreadStart(doDequeue));
                //thread.Start();
            }
        }

         /// <summary>
        /// dequeue procedure
        /// </summary>
        private void doDequeue()
        {
            while (true)
            {
                if (_storingItems.Count == 0) break;

                string itemkey= null;
                lock (_syncObject)
                {
                    if (_storingItems.Count == 0) break;
                    
                    //dequeue an item
                    itemkey = _storingItems.Dequeue();
                }

                //notify the cache manager to store
                if (itemkey!=null && StoreReady != null)
                {
                    StoreReadyEventArgs args = new StoreReadyEventArgs(itemkey);
                    StoreReady(this, args);
                }
            }
            
            //reset the flag
            _isStoring = false;
        }

        /// <summary>
        /// Add new item to queue
        /// </summary>
        /// <param name="itemkey"></param>
        public void AddItem(string itemkey)
        {
            lock (_syncObject)
            {
                if (_storingItems.Contains(itemkey))
                {
                    return; //item key has already in queue
                }

                _storingItems.Enqueue(itemkey);
            }
        }

        /// <summary>
        /// Waiting for all items have been stored
        /// </summary>
        public void CloseAndWaitForComplete()
        {
            _scanTimer.Stop();

            //store remain items
            scanTimer_Elapsed(this, null);
        }
    }

}
