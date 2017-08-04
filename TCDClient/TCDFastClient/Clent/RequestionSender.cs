using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;
using Doms.TCClient.Configuration;

namespace Doms.TCClient.Client
{
    class RequestionSender
    {
        private List<KeyValuePair<string, SessionCollection>> _servers;

        private static RequestionSender _instance = new RequestionSender();

        private RequestionSender()
        {
            int maxConnections = TCClientConfigSection.Instance.Connection.MaxConnections;
            int timeout = TCClientConfigSection.Instance.Connection.Timeout * 1000;

            _servers = new List<KeyValuePair<string, SessionCollection>>();

            //add persistance servers
            foreach (ServerEndpointConfigElement node in TCClientConfigSection.Instance.ServerNodes)
            {
                SessionCollection sc = new SessionCollection(node.ToEndPoint(), timeout, maxConnections);
                KeyValuePair<string, SessionCollection> kvp = new KeyValuePair<string, SessionCollection>
                    (node.ToString(), sc);
                _servers.Add(kvp);
            }

            //add non-persistance servers
            foreach (NonPersistentConfigElement node in TCClientConfigSection.Instance.NonPersistentNodes)
            {
                SessionCollection sc = new SessionCollection(node.ToEndPoint(), timeout, maxConnections);
                KeyValuePair<string, SessionCollection> kvp = new KeyValuePair<string, SessionCollection>
                    (node.ToString(), sc);
                _servers.Add(kvp);
            }
        }

        public static RequestionSender Instance
        {
            get { return _instance; }
        }

        public HttpResponseHeader Send(
            string serverEndPoint,
            HttpRequestHeader header,
            byte[] bodyData,
            out byte[] responseData)
        {
            SessionCollection col = null;
            for (int idx = 0; idx < _servers.Count; idx++)
            {
                if (_servers[idx].Key == serverEndPoint)
                {
                    col = _servers[idx].Value;
                    break;
                }
            }

            if (col == null)
            {
                throw new InvalidOperationException("The specify server endpoint not found");
            }

            bool reconnected = false;

            while (true)
            {
                int sid = 0;
                try
                {
                    TCPSession session = col.GetSession(out sid);
                    HttpResponseHeader response = session.Send(header, bodyData, out responseData);
                    col.ReleaseSession(sid);
                    return response;
                }
                catch (ConnectionsOverflowException ex)
                {
                    //no enought idle session
                    //TODO:: logger
                    throw ex;
                }
                catch (SocketException ex)
                {
                    col.ReleaseAndDisposeSession(sid); //release session and dispose it

                    if (ex.SocketErrorCode == SocketError.ConnectionAborted ||
                        ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        if (reconnected)
                        {
                            throw ex;
                        }
                        else
                        {
                            //need re-connect, and try once again
                            reconnected = true;
                            continue;
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (InvalidDataException ex)
                {
                    col.ReleaseAndDisposeSession(sid); //release session and dispose it
                    if (reconnected)
                    {
                        throw ex;
                    }
                    else
                    {
                        //need re-connect, and try once again
                        reconnected = true;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    col.ReleaseAndDisposeSession(sid); //release session and dispose it
                    throw ex;
                }
            }
        }


    }//end class
}
