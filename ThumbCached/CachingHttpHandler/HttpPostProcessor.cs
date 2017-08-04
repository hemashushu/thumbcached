using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Doms.HttpService.HttpProtocol;
using Doms.HttpService.HttpHandler;
using Doms.ThumbCached.Caching;

namespace Doms.ThumbCached.CachingHttpHandler
{
    public class HttpPostProcessor : IHttpRequestProcessor
    {
        private HttpResponseHeader _responseHeader;
        private CacheManagerCollection _cacheManCollection;
        private CacheManager _cacheManager;
        private MemoryStream _requestStream;
        private bool _canAcceptRequestContent;

        //item info
        private string _itemKey;
        private DateTime _itemTime;
        private int _itemExpirationSecond;
        private bool _itemAbsExpire;
        private int _itemProperties;

        public void SetManagerCollection(CacheManagerCollection col)
        {
            _cacheManCollection = col;
        }

        #region IHttpRequestProcessor Members

        public void ProcessRequest(HandlerContext context)
        {
            _responseHeader = context.ResponseHeader;
            _responseHeader.Server += ";\x20" + ThumbCachedHttpHandler.HANDLER_NAME;
            _responseHeader.ContentLength = 0;

            TcdUrlParser urlParser = null;
            try
            {
                urlParser = new TcdUrlParser(context.RequestHeader.Url);
            }
            catch
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            try
            {
                _cacheManager = _cacheManCollection.GetCacheManager(context.BindEndPointName);
            }
            catch
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (string.IsNullOrEmpty(urlParser.ItemKey))
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (urlParser.Action == ActionType.Update)
            {
                _canAcceptRequestContent = true;

                _requestStream = new MemoryStream();
                
                //get item info
                _itemKey = urlParser.ItemKey;
                _itemExpirationSecond = urlParser.ExpirationSecond;
                _itemAbsExpire = urlParser.AbsoluteExpire;

                string lastModified = context.RequestHeader.Headers.Get("Last-Modified");
                if (lastModified != null)
                {
                    _itemTime = DateTime.Parse(lastModified).ToLocalTime(); //conver to local time
                }

                string contentType =context.RequestHeader.ContentType;
                if (contentType != null)
                {
                    int pos = contentType.IndexOf('/');
                    if (pos > 0)
                    {
                        _itemProperties = int.Parse(contentType.Substring(pos + 1));
                    }
                }
                
            }
            else
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
            }
        }

        public bool RequestBodyAcceptable
        {
            get { return _canAcceptRequestContent; }
        }

        public void RequestBodyArrival(byte[] buffer, int length)
        {
            _requestStream.Write(buffer, 0, length);
        }

        public void AllRequestBodyReceived()
        {
            try
            {
                byte[] content = _requestStream.ToArray();

                CacheItem item = new CacheItem(
                    _itemKey, content, _itemTime, _itemProperties);

                //store
                _cacheManager.Set(item, _itemExpirationSecond, _itemAbsExpire);
                _responseHeader.Status = System.Net.HttpStatusCode.OK;
            }
            catch
            {
                _responseHeader.Status = System.Net.HttpStatusCode.InternalServerError;
            }

            _requestStream.Close();
        }

        public HttpResponseHeader ResponseHeader
        {
            get {return _responseHeader; }
        }

        public long ResponseBodyLength
        {
            get { return 0; }
        }

        public int SubmitResponseBody(out byte[] data, out int offset)
        {
            throw new InvalidOperationException("No response content");
        }

        public void Close()
        {
            if (_requestStream != null)
            {
                _requestStream.Close();
            }
        }

        #endregion

    }
}
