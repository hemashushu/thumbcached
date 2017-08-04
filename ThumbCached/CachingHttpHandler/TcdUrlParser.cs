using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.CachingHttpHandler
{
    class TcdUrlParser
    {
        //item key
        private string _itemKey;

        //item keys in multi-get requestion
        private string[] _itemKeys;

        //the specify expiration time (in second), 
        //NOTE:: 0 = no expiration
        private int _expirationSecond;

        private bool _absoluteExpire;

        //operation type
        private ActionType _action;

        public TcdUrlParser(string url)
        {
            //URL sample:
            // GET /fetch/item_key
            // GET /multifetch/?keys=abc,def,xyz
            // GET /remove/item_key
            // GET /status
            // POST /update/item_key + BODY DATA
            // POST /update/item_key?expire=123456?abs=1 + BODY DATA

            //get the query value
            int queryPos = url.IndexOf('?');
            if (queryPos > 0)
            {
                string[] queries = url.Substring(queryPos + 1).Split('&');
                foreach (string queryItem in queries)
                {
                    if (queryItem.Length == 0) continue;

                    #region parse query item
                    int equalsPos = queryItem.IndexOf('=');
                    if (equalsPos > 1 && equalsPos < queryItem.Length - 1)
                    {
                        string queryValue = queryItem.Substring(equalsPos + 1);

                        switch (queryItem.Substring(0, equalsPos))
                        {
                            case "expire":
                                _expirationSecond = int.Parse(queryValue);
                                break;

                            case "keys":
                                _itemKeys = queryValue.Split(',');
                                break;

                            case "abs":
                                _absoluteExpire = (queryValue == "1");
                                break;

                        }
                    }
                    #endregion
                }

                url = url.Substring(0, queryPos);
            }

            //get the item key and action
            string actionName = null;
            int actionPos = url.IndexOf('/', 1);

            if (actionPos > 1)
            {
                actionName = url.Substring(1, actionPos - 1);
            }
            else
            {
                actionName = url.Substring(1);
            }

            switch (actionName)
            {
                case "fetch":
                    _action = ActionType.Fetch;
                    _itemKey = Uri.UnescapeDataString(url.Substring(actionPos + 1));
                    break;

                case "multifetch":
                    _action = ActionType.MultiFetch;
                    break;

                case "status":
                    _action = ActionType.Status;
                    break;

                case "update":
                    _action = ActionType.Update;
                    _itemKey = Uri.UnescapeDataString(url.Substring(actionPos + 1));
                    break;

                case "remove":
                    _action = ActionType.Remove;
                    _itemKey = Uri.UnescapeDataString(url.Substring(actionPos + 1));
                    break;

                default:
                    throw new InvalidOperationException("No this action");
            }
        }

        #region query value
        public string ItemKey
        {
            get { return _itemKey; }
        }

        public string[] ItemKeys
        {
            get { return _itemKeys; }
        }

        public ActionType Action
        {
            get { return _action; }
        }

        public int ExpirationSecond
        {
            get { return _expirationSecond; }
        }

        public bool AbsoluteExpire
        {
            get { return _absoluteExpire; }
        }
        #endregion

    }

    enum ActionType
    {
        Fetch,
        MultiFetch,
        Update,
        Remove,
        Status
    }
}

