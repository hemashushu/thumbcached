using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Doms.ThumbCached.Caching;
using Doms.ThumbCached.CachingHttpHandler;
using Doms.HttpService;

namespace Doms.ThumbCached.Service
{
    public partial class ThumbCachedService : ServiceBase
    {
        private CacheManagerCollection _cacheManCollection;
        private HttpServiceControler _webServer;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ThumbCachedService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //create cache managers and web server
                _cacheManCollection = new CacheManagerCollection();
                _webServer = new HttpServiceControler();

                //add handler
                ThumbCachedHttpHandler handler = new ThumbCachedHttpHandler();
                handler.SetManagerCollection(_cacheManCollection);
                _webServer.AddHandler(handler);
                _webServer.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw new ApplicationException("Starting ThumbCached service fail: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            //stop server
            _webServer.Stop();
            _cacheManCollection.Close();
        }
    }
}
