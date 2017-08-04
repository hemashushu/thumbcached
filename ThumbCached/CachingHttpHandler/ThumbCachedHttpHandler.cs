using System;
using System.Collections.Generic;
using System.Text;
using Doms.HttpService.HttpProtocol;
using Doms.HttpService.HttpHandler;
using Doms.ThumbCached.Caching;

namespace Doms.ThumbCached.CachingHttpHandler
{
    public class ThumbCachedHttpHandler:IHttpHandler
    {
        public const string HANDLER_NAME = "ThumbCached/1.0";

        private CacheManagerCollection _cacheManCollection;

        public void SetManagerCollection(CacheManagerCollection col)
        {
            _cacheManCollection = col;
        }

        #region IHttpHandler Members

        public IHttpRequestProcessor CreatProcessor(HandlerContext context)
        {
            switch (context.RequestHeader.Method)
            {
                case HttpMethods.GET:
                    HttpGetProcessor httpGet = new HttpGetProcessor();
                    httpGet.SetManagerCollection(_cacheManCollection);
                    return httpGet;

                case HttpMethods.POST:
                    HttpPostProcessor httpPost = new HttpPostProcessor();
                    httpPost.SetManagerCollection(_cacheManCollection);
                    return httpPost;
            }

            return null;
        }

        public void Close()
        {
            //do nothing
        }
        #endregion

    }
}
