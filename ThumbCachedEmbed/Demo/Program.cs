using System;
using System.Collections.Generic;
using System.Text;
using Doms.ThumbCached.Embed;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        private static void Test()
        {

            //create instance
            TCache cache = new TCache("tcd001"); //or cache = TCache.Nodes("c001");

            //add & get cache
            cache["item01"] = "Hello world";
            string v1 = (string)cache["item01"];
            Console.WriteLine(v1);

            cache["item02"] = "abc";
            cache["item03"] = DateTime.Now;
            cache["item04"] = 3.14159;

            Console.WriteLine(cache["item02"]);
            Console.WriteLine(cache["item03"]);
            Console.WriteLine(cache["item04"]);

            //add cache items with values
            cache.Add("item10", "Foo");
            cache.Add("item11", "Bar", DateTime.Now); //specify item's time property
            cache.Add("item13", "Test text", TCache.NoItemTime);

            string v2 = (string)cache.Get("item11");
            string v3 = (string)cache.Get("item11", DateTime.Now.AddSeconds(-3)); //check the item time if modified since the specify time

            try
            {
                //the follow line will raise CacheItemNotModifyException
                string v4 = (string)cache.Get("item11", DateTime.Now.AddSeconds(3));
                Console.WriteLine("ERROR");
            }
            catch (CacheItemNotModifiedException ex)
            {
                Console.WriteLine("test specify time... PASS, actually time:{0}", ex.LastModifyTime);
            }

            try
            {
                //the follow line will raise CacheItemNotFoundException
                object v5 = cache.Get("item999");
                Console.WriteLine("ERROR");
            }
            catch (CacheItemNotFoundException)
            {
                Console.WriteLine("test specify key that does not exis ... PASS");
            }

            //get cache item detail infomation
            TCacheItem item = cache.GetItem("item11");
            Console.WriteLine("Key:{0} value:{1} item time:{2}",
                item.Key, item.Value, item.ItemTime);

            //multi get
            TCacheItem[] items = cache.MultiGet(new string[] { "item10", "item11", "item12", "item13" });
            foreach (TCacheItem ci in items)
            {
                Console.WriteLine("Key:{0} value:{1}", ci.Key, ci.Value, ci.ItemTime);
            }

            //remove
            cache.Remove("item11");
            try
            {
                //the follow line will raise CacheItemNotFoundException
                string v6 = (string)cache.Get("item11");
                Console.WriteLine("ERROR");
            }
            catch (CacheItemNotFoundException)
            {
                Console.WriteLine("test remove item... PASS");
            }

            Console.WriteLine("DEMO END");
            Console.ReadLine();
        }

    }

    [Serializable]
    class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
