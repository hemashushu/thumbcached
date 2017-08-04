using System;
using System.Collections.Generic;
using System.Text;
using Doms.ThumbCached.Caching;
using Doms.ThumbCached.CachingHttpHandler;
using Doms.HttpService;

namespace Doms.ThumbCached.Server
{
    class Program
    {
        private CacheManagerCollection _cacheManCollection;
        private HttpServiceControler _webServer;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Program p = new Program();
            try
            {
                p.Start();
                p.Stop();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        public void Start()
        {
            //create cache managers and web server
            _cacheManCollection = new CacheManagerCollection();
            _webServer = new HttpServiceControler();

            //add handler
            ThumbCachedHttpHandler handler = new ThumbCachedHttpHandler();
            handler.SetManagerCollection(_cacheManCollection);
            _webServer.AddHandler(handler);
            _webServer.Start();

            Console.WriteLine("ThumbCached server started!");

            //show status
            while (true)
            {
                string cmd = Console.ReadLine();
                if (string.Compare(cmd, "quit", true) == 0) break;
                Console.WriteLine("Enter \"quit\" or press Ctrl+C to exit program.");
                Console.WriteLine("Network connections: {0}", _webServer.Connections);
            }
        }

        public void Stop()
        {
            //stop server
            _webServer.Stop();
            _cacheManCollection.Close();
        }
    }
}

//Copyright (c) 2007-2009, Kwanhong Young, All rights reserved.
//mapleaves@gmail.com
//http://www.domstorage.com