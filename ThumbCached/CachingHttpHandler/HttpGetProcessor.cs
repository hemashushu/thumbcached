using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Doms.HttpService.HttpProtocol;
using Doms.HttpService.HttpHandler;
using Doms.ThumbCached.Caching;

namespace Doms.ThumbCached.CachingHttpHandler
{
    public class HttpGetProcessor:IHttpRequestProcessor
    {
        private HttpResponseHeader _responseHeader;
        private CacheManagerCollection _cacheManCollection;
        private CacheManager _cacheManager;
        
        private byte[] _responseBody;
        private bool _firstSubmitResponseBody;
        private int _contentLength;

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

            _firstSubmitResponseBody = true;

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

            if ((urlParser.Action== ActionType.Fetch ||
                urlParser.Action== ActionType.Remove) && 
                string.IsNullOrEmpty(urlParser.ItemKey))
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
                return;
            }

            if (urlParser.Action == ActionType.Remove)
            {
                #region remove cache item
                try
                {
                    _cacheManager.Remove(urlParser.ItemKey);
                    _responseHeader.Status = System.Net.HttpStatusCode.OK;
                }
                catch
                {
                    _responseHeader.Status = System.Net.HttpStatusCode.InternalServerError;
                }
                #endregion
            }
            else if (urlParser.Action == ActionType.Fetch)
            {
                #region fetch cache item
                try
                {
                    CacheItem item = _cacheManager.Get(urlParser.ItemKey);

                    long span = (long)(item.ItemTime - context.RequestHeader.IfModifiedSince).TotalSeconds;
                    if (span > 0)
                    {
                        _responseHeader.Status = System.Net.HttpStatusCode.OK;
                        _responseHeader.ContentType = "tcd/" + item.Properties ;
                        _responseHeader.ContentLength = item.Content.Length;
                        _responseHeader.LastModified = item.ItemTime.ToUniversalTime(); //use universal time

                        _contentLength = item.Content.Length;
                        _responseBody = item.Content;
                    }
                    else //Content not modify since the specify time
                    {
                        _responseHeader.Status = System.Net.HttpStatusCode.NotModified;
                        _responseHeader.ContentType = "tcd/" + item.Properties;
                        _responseHeader.LastModified = item.ItemTime.ToUniversalTime(); //use universal time
                    }
                }
                catch (ItemNotFoundException)
                {
                    _responseHeader.Status = System.Net.HttpStatusCode.NotFound;
                }
                #endregion
            }
            else if (urlParser.Action == ActionType.MultiFetch)
            {
                #region mulit-get items
                using (MemoryStream ms = new MemoryStream())
                {
                    foreach (string key in urlParser.ItemKeys)
                    {
                        try
                        {
                            CacheItem item = _cacheManager.Get(key);
                            string header = item.Key + "\r\n" +
                                            item.Properties + "\r\n" +
                                            item.ItemTime.ToUniversalTime().ToString() + "\r\n" + //use universal time
                                            item.Content.Length + "\r\n\r\n";
                            ms.Write(Encoding.ASCII.GetBytes(header), 0, header.Length);
                            ms.Write(item.Content, 0, item.Content.Length);
                            ms.WriteByte(13);
                            ms.WriteByte(10);
                        }
                        catch (ItemNotFoundException)
                        {
                            //ignore this error
                        }
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    _responseHeader.Status = System.Net.HttpStatusCode.OK;
                    _responseHeader.ContentLength = ms.Length;

                    _contentLength = (int)ms.Length;
                    _responseBody = ms.GetBuffer();

                    ms.Close();
                }
                #endregion
            }
            else if (urlParser.Action == ActionType.Status)
            {
                #region get status
                StringBuilder ss = new StringBuilder(8);
                ss.Append("Pool memory: ");
                ss.Append(_cacheManager.MemorySize + "\r\n");
                ss.Append("Used memory: ");
                ss.Append(_cacheManager.UsedMemorySize + "\r\n");
                ss.Append("Item amount: ");
                ss.Append(_cacheManager.CacheCount + "\r\n");
                ss.Append("Item hits: ");
                ss.Append(_cacheManager.CacheTotalHits + "\r\n");

                byte[] data = Encoding.ASCII.GetBytes(ss.ToString());

                _responseHeader.Status = System.Net.HttpStatusCode.OK;
                _responseHeader.ContentType = "text/plain";
                _responseHeader.ContentLength = data.Length;

                _contentLength = data.Length;
                _responseBody = data;
                #endregion
            }
            else
            {
                _responseHeader.Status = System.Net.HttpStatusCode.BadRequest;
            }
        }

        public bool RequestBodyAcceptable
        {
            get { return false; }
        }

        public void RequestBodyArrival(byte[] buffer, int length)
        {
            throw new InvalidOperationException("Can not append request content");
        }

        public void AllRequestBodyReceived()
        {
            throw new InvalidOperationException("Can not append request content");
        }

        public HttpResponseHeader ResponseHeader
        {
            get { return _responseHeader; }
        }

        public long ResponseBodyLength
        {
            get { return _contentLength; }
        }

        public int SubmitResponseBody(out byte[] data, out int offset)
        {
            if (_firstSubmitResponseBody)
            {
                _firstSubmitResponseBody = false;
                data = _responseBody;
                offset = 0;
                return _contentLength;
            }
            else
            {
                data = null;
                offset = 0;
                return 0;
            }
        }

        public void Close()
        {
            if (_responseBody != null)
            {
                _responseBody=null;
            }
        }

        #endregion

    }
}
