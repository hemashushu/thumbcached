using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Doms.TCClient.Client
{
    class SessionCollection
    {
        private int _maxConnections;
        private bool[] _busy;
        private TCPSession[] _sessions;

        private IPEndPoint _endPoint;
        private int _timeout;

        private object _syncObject = new object();


        public SessionCollection(IPEndPoint ep, int timeout, int maxconns)
        {
            _endPoint = ep;
            _timeout = timeout;
            _maxConnections = maxconns;

            _busy = new bool[_maxConnections];
            _sessions = new TCPSession[_maxConnections];
        }

        public TCPSession GetSession(out int sid)
        {
            lock (_syncObject)
            {
                int idx = 0;

                    for (; idx < _maxConnections; idx++)
                    {
                        if (!_busy[idx])
                        {
                            _busy[idx] = true; //set busy
                            break;
                        }
                    }

                sid = idx;

                if (sid >= _maxConnections)
                {
                    //no enought idle session
                    throw new ConnectionsOverflowException();
                }

                if (_sessions[sid] == null)
                { 
                    _sessions[sid] = new TCPSession(_endPoint, _timeout); //this may raise socket connection refuse exception
                }

                return _sessions[sid];
            }
        }

        public void ReleaseSession(int sid)
        {
            lock (_syncObject)
            {
                _busy[sid] = false; //release
            }
        }

        public void ReleaseAndDisposeSession(int sid)
        {
            lock (_syncObject)
            {
                _busy[sid] = false; //release
                
                TCPSession session = _sessions[sid];

                if (session != null)
                {
                    session.Dispose();
                    _sessions[sid] = null;
                }
            }
        }


    }
}
