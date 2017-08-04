using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.TCClient.Client
{
    class HttpRequestHeader
    {
        private string _method;
        private string _url;
        private string _host;
        private long _contentLength;
        private string _contentType;

        private DateTime _ifModifiedSince;
        private DateTime _lastModified;

        private static Encoding _defaultEncoding = Encoding.ASCII;

        public const string Method_Get = "GET";
        public const string Method_Post = "POST";

        public HttpRequestHeader()
        {
            _method = Method_Get;
        }

        public byte[] GetHeaderRawData()
        {
            return combineHeaders();
        }

        #region request headers value
        public string Method
        {
            get { return _method; }
            set { _method = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url=value; }
        }

        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        public long ContentLength
        {
            get { return _contentLength; }
            set { _contentLength = value; }
        }

        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        public DateTime IfModifiedSince
        {
            get { return _ifModifiedSince; }
            set { _ifModifiedSince = value; }
            
            //NOTE::
            //use GMT DataTime format,see RFC 822, updated by RFC 1123
            
            //Example:
            // Sun, 06 Nov 1994 08:49:37 GMT
            //_headers.Set("If-Modified-Since", value.ToString("r"));
        }

        public DateTime LastModified
        {
            get { return _lastModified; }
            set { _lastModified = value; }
        }

        #endregion


        private byte[] combineHeaders()
        {
            StringBuilder lines = new StringBuilder(128);
            lines.Append(_method + " " + _url + " HTTP/1.1\r\n");
            lines.Append("Host: " + _host +"\r\n");
            lines.Append("Content-Length: " + _contentLength + "\r\n");

            if (_contentType != null)
            {
                lines.Append("Content-Type: " + _contentType + "\r\n");
            }

            if (_ifModifiedSince != DateTime.MinValue)
            {
                lines.Append("If-Modified-Since: " + _ifModifiedSince.ToString("r") + "\r\n");
            }

            if (_lastModified != DateTime.MinValue)
            {
                lines.Append("Last-Modified: " + _lastModified.ToString("r") + "\r\n");
            }

            lines.Append("\r\n");

            return _defaultEncoding.GetBytes(lines.ToString());
        }

    }
}
