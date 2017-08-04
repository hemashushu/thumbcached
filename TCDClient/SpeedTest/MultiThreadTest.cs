using System;
using System.Collections.Generic;
using System.Text;
using Doms.TCClient;
using System.Threading;

namespace SpeedTest
{
    class MultiThreadTest
    {
        private List<string> _sourceData;
        private List<bool> _threadComplete;
        private ManualResetEvent _locker;

        public const int ITEM_AMOUNT = 200;
        public const int THREAD_NUMBER = 8;
        public const int REPEAT_TIMES = 30;

        public void Test()
        {
            _threadComplete = new List<bool>();
            for (int idx = 0; idx < THREAD_NUMBER; idx++)
            {
                _threadComplete.Add(false);
            }

            _sourceData = new List<string>();
            _locker = new ManualResetEvent(false);

            generateData();

            int itemsLength = 0;
            foreach (string item in _sourceData)
            {
                itemsLength += item.Length;
            }
            Console.WriteLine("items average length: {0:N} byte", (double)itemsLength / ITEM_AMOUNT);

            //store first
            Console.WriteLine("adding...");
            storeItems();

            //start get
            Console.WriteLine("starting {0} threads to get items.", THREAD_NUMBER);
            Console.WriteLine("each thread get {0} items, repeat {1} times.", ITEM_AMOUNT, REPEAT_TIMES);

            DateTime start = DateTime.Now;
            for (int idx = 0; idx < THREAD_NUMBER; idx++)
            {
                Worker worker = new Worker();
                worker.Number = idx;
                worker.Complete += new EventHandler(worker_Complete);

                Console.WriteLine("thread {0} start", idx);
                ThreadPool.QueueUserWorkItem(new WaitCallback(worker.GetItemsWithRepeat));
            }

            _locker.WaitOne();

            TimeSpan span = DateTime.Now - start;
            Console.WriteLine("total used time: {0:N} milliseconds", span.TotalMilliseconds);
            Console.WriteLine("access speed: {0:N} items/second", (double)THREAD_NUMBER * REPEAT_TIMES * ITEM_AMOUNT / span.TotalSeconds);
            Console.WriteLine("test complete");

            Console.ReadLine();
        }

        void worker_Complete(object sender, EventArgs e)
        {
            Worker w = (Worker)sender;
            Console.WriteLine(">>>>> thread {0} complete", w.Number);
            _threadComplete[w.Number] = true;

            int idx = 0;
            for (; idx < THREAD_NUMBER; idx++)
            {
                if (_threadComplete[idx] == false) break;
            }

            if (idx == THREAD_NUMBER)
            {
                _locker.Set();
            }
        }

        private void generateData()
        {
            Random ran = new Random();

            //generate source data
            for (int idx = 0; idx < ITEM_AMOUNT; idx++)
            {
                StringBuilder str = new StringBuilder();
                int length = ran.Next(200, 600);
                for (int i = 0; i < length; i++)
                {
                    str.Append((char)ran.Next(65, 91)); //a-z
                }
                _sourceData.Add(str.ToString());
            }
        }

        private void storeItems()
        {
            for (int idx = 0; idx < ITEM_AMOUNT; idx++)
            {
                TCacheNP.Add(idx.ToString(), _sourceData[idx]);
            }
        }
    }

    class Worker
    {
        public int Number { get; set; }
        public event EventHandler Complete;

        public void GetItemsWithRepeat(object o)
        {
            for (int rep = 0; rep < MultiThreadTest.REPEAT_TIMES; rep++)
            {
                Console.WriteLine("thread {0} processing {1}/{2}...", Number, rep, MultiThreadTest.REPEAT_TIMES);
                for (int idx = 0; idx < MultiThreadTest.ITEM_AMOUNT; idx++)
                {
                    try
                    {
                        object val = TCacheNP.Get(idx.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            "fail in thread {0} in repeat {1}, item key:{2}, exception message:{3}",
                            Number, rep, idx, ex.Message);
                    }
                }
            }

            if (Complete != null)
            {
                Complete(this, EventArgs.Empty);
            }
        }
    }
}
