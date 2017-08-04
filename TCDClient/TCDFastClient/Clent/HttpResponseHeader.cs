using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.TCClient.Client
{
    class HttpResponseHeader
    {
        private int _statusCode;
        private string _contentType;
        private long _contentLength;
        private DateTime _lastModified;

        private static Encoding _defaultEncoding = Encoding.ASCII;

        public void SetHeaderRawData(byte[] data, int offset, int count)
        {
            parseHeader(data, offset, count);
        }

        #region response headers value
        public int StatusCode
        {
            get { return _statusCode; }
        }

        public string ContentType
        {
            get { return _contentType; }
        }

        public long ContentLength
        {
            get { return _contentLength; }
        }

        public DateTime LastModified
        {
            get { return _lastModified; }
        }
        #endregion

        private void parseHeader(byte[] data, int offset, int count)
        {
            if (data.Length == 0 || offset >= count || count == 0)
                throw new ArgumentException("Invalid header data");

            //split into lines
            string[] lines = _defaultEncoding.GetString(data, offset, count).Split(new string[] { "\r\n" }, StringSplitOptions.None);

            //first line
            string firstLine = lines[0];
            int pos = firstLine.IndexOf('\x20');
            if (pos > 0 && pos < firstLine.Length - 1)
            {
                int pos2 = firstLine.IndexOf('\x20', pos + 1);
                if (pos2 > 0 && pos2 < firstLine.Length - 1)
                {
                    _statusCode = int.Parse(firstLine.Substring(pos + 1, pos2 - pos - 1));
                }
            }

            if (_statusCode == 0)
            {
                throw new InvalidCastException("Status code error");
            }

            for (int idx = 1; idx < lines.Length; idx++)
            {
                int pos1 = lines[idx].IndexOf(':');
                if (pos1 > 0)
                {
                    string headerName = lines[idx].Substring(0, pos1);
                    string headerValue = lines[idx].Substring(pos1 + 1); //.Trim();

                    switch (headerName)
                    {
                        case "Content-Type":
                            _contentType = headerValue;
                            break;
                        case "Content-Length":
                            _contentLength = long.Parse(headerValue);
                            break;
                        case "Last-Modified":
                            _lastModified = DateTime.Parse(headerValue);
                            break;
                    }
                }
            }
        }

    }//end class
}
