using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Doms.TCClient.Client;

namespace Doms.TCClient
{
    class Proxy
    {
        public static void Add(
            string serverEp, TCacheItem item, int expirationSecond, bool absExpire)
        {
            //serialize data
            byte[] data;
            CacheDataType dataType;
            DataSerialize.Serialize(item.Value, out data, out dataType);

            //url
            string url = "/update/" + Uri.EscapeDataString(item.Key);
            if (expirationSecond > 0)
            {
                url += "?expire=" + expirationSecond + "&abs=" + (absExpire ? "1" : "0");
            }

            //request header
            HttpRequestHeader request = new HttpRequestHeader();
            request.Method = HttpRequestHeader.Method_Post;
            request.Url = url;
            request.Host = serverEp;
            request.ContentLength = data.Length;
            request.ContentType = "tcd/" + (int)dataType;
            if (item.ItemTime > DateTime.MinValue)
            {
                request.LastModified = item.ItemTime.ToUniversalTime(); //use universal time
            }

            byte[] buffer;
            HttpResponseHeader response = RequestionSender.Instance.Send(
                serverEp, request, data, out buffer);

        }

        public static TCacheItem Get(string serverEp, string key, DateTime ifModifySince)
        {
            //url
            string url = "/fetch/" + Uri.EscapeDataString(key);

            //request header
            HttpRequestHeader request = new HttpRequestHeader();
            request.Method = HttpRequestHeader.Method_Get;
            request.Url = url;
            request.Host = serverEp;

            if (ifModifySince != DateTime.MinValue)
            {
                request.IfModifiedSince = ifModifySince.ToUniversalTime(); //user universal time
            }

            byte[] buffer;
            HttpResponseHeader response = RequestionSender.Instance.Send(
                serverEp, request, null, out buffer);


            if (response.StatusCode == 200)
            {
                int properties = 0;
                string contentType = response.ContentType;
                int pos = contentType.IndexOf('/');
                if (pos > 0)
                {
                    properties = int.Parse(contentType.Substring(pos + 1));
                }

                DateTime lastModifiedTime = response.LastModified.ToLocalTime(); //convert to local time

                object val = DataSerialize.Deserialize(buffer, (CacheDataType)properties);
                return new TCacheItem(key, val, lastModifiedTime);
            }
            else if (response.StatusCode == 404)
            {
                throw new CacheItemNotFoundException();
            }
            else if (response.StatusCode == 304)
            {
                throw new CacheItemNotModifiedException(response.LastModified.ToLocalTime()); //convert to local time
            }
            else
            {
                throw new System.Net.WebException("Status code: " + response.StatusCode);
            }
        }

        public static TCacheItem[] MultiGet(string serverEp, string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                throw new ArgumentNullException("keys");
            }

            //url
            for (int idx = 0; idx < keys.Length; idx++)
            {
                keys[idx] = Uri.EscapeDataString(keys[idx]);
            }
            string url = "/multifetch/?keys=" + String.Join(",", keys);

            //request header
            HttpRequestHeader request = new HttpRequestHeader();
            request.Method = HttpRequestHeader.Method_Get;
            request.Url = url;
            request.Host = serverEp;


            byte[] buffer;

            HttpResponseHeader response = RequestionSender.Instance.Send(
                serverEp, request, null, out buffer);

            if (response.StatusCode != 200)
            {
                throw new System.Net.WebException("Status code: " + response.StatusCode);
            }

            if (buffer.Length == 0)
            {
                return new TCacheItem[0];
            }

            //parse items
            List<TCacheItem> items = new List<TCacheItem>();
            int itemPos = 0;
            while (itemPos < buffer.Length)
            {
                int headerPos = seekSplitSymbolPosition(buffer, itemPos);
                if (headerPos == -1)
                {
                    break;
                }
                else
                {
                    string header = Encoding.ASCII.GetString(buffer, itemPos, headerPos - itemPos);
                    string[] headers = header.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    string itemkey = headers[0];
                    int itemProperties = int.Parse(headers[1]);
                    DateTime itemTime = DateTime.Parse(headers[2]).ToLocalTime(); //convert to local time
                    int itemLength = int.Parse(headers[3]);

                    byte[] itemData = new byte[itemLength];
                    Buffer.BlockCopy(buffer, headerPos + 4, itemData, 0, itemLength);
                    object itemValue = DataSerialize.Deserialize(itemData, (CacheDataType)itemProperties);
                    items.Add(new TCacheItem(itemkey, itemValue, itemTime));


                    //next item
                    itemPos = headerPos + 4 + itemLength + 2;
                }
            }

            return items.ToArray();


        }


        private static int seekSplitSymbolPosition(byte[] source, int startIndex)
        {
            //the split-symbol is double "\r\n"
            byte[] splitSymbol = new byte[] { 0xd, 0xa, 0xd, 0xa };

            for (int idx = startIndex; idx < source.Length; idx++)
            {
                int offset = 0;
                for (; offset < 4 && idx + offset < source.Length; offset++)
                {
                    if (source[idx + offset] != splitSymbol[offset]) break;
                }
                if (offset == 4)
                {
                    return idx;
                }
            }
            return -1;
        }

        public static void Remove(string serverEp, string key)
        {
            string url = "/remove/" + Uri.EscapeDataString(key);

            //request header
            HttpRequestHeader request = new HttpRequestHeader();
            request.Method = HttpRequestHeader.Method_Get;
            request.Url = url;
            request.Host = serverEp;

            byte[] buffer;
            HttpResponseHeader response = RequestionSender.Instance.Send(
                serverEp, request, null, out buffer);
        }

        public static string GetServerStatus(string serverEp)
        {
            string url = "/status";

            //request header
            HttpRequestHeader request = new HttpRequestHeader();
            request.Method = HttpRequestHeader.Method_Get;
            request.Url = url;
            request.Host = serverEp;

            byte[] buffer;
            HttpResponseHeader response = RequestionSender.Instance.Send(
                serverEp, request, null, out buffer);

            string content = Encoding.ASCII.GetString(buffer);
            return content;
        }

    }//end class
}
