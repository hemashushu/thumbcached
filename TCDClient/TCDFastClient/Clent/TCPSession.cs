using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Doms.TCClient.Client
{

    class TCPSession:IDisposable
    {
        private Socket _socket;
        private byte[] _receiveHeaderBuffer;
        private bool _disposed; //a flag to remember dispose

        public TCPSession(IPEndPoint ep, int timeout)
        {
            _socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            _socket.NoDelay = true; //donot use Nagle 
            _socket.SendTimeout = timeout;
            _socket.ReceiveTimeout = timeout;
            _socket.ReceiveBufferSize = 64;

            _socket.Connect(ep); //this may raise SocketError.ConnectionRefused

            _receiveHeaderBuffer = new byte[4  * 1024];
        }

        #region dispose
        ~TCPSession()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //
                }

                if (_socket.Connected)
                {
                    _socket.Close();
                }

                _disposed = true;
            }
        }
        #endregion

        public HttpResponseHeader Send(HttpRequestHeader header, byte[] bodyData, out byte[] responseData)
        {
            //this function maybe raise these exception:
            //  SocketError.ConnectionReset
            //  SocketError.ConnectionAborted
            
            byte[] headData = header.GetHeaderRawData();
            _socket.Send(headData);
            
            if (bodyData != null && bodyData.Length > 0)
            {
                _socket.Send(bodyData);
            }

            //receive
            bool findingHeader = true;
            int receiveOffset = 0;
            
            HttpResponseHeader response = new HttpResponseHeader();
            responseData = null;

            while (true)
            {
                if (findingHeader)
                {
                    #region receive header

                    if (receiveOffset >= _receiveHeaderBuffer.Length)
                    {
                        throw new InvalidDataException("Header data overflow");
                    }

                    int recByte = _socket.Receive(
                        _receiveHeaderBuffer,
                        receiveOffset,
                        _receiveHeaderBuffer.Length - receiveOffset,
                        SocketFlags.None);

                    if (recByte == 0)
                    {
                        throw new InvalidDataException("Invalid header data");
                    }

                    receiveOffset += recByte;

                    //contain body data
                    int headPos = seekSplitSymbolPosition(_receiveHeaderBuffer, 0, receiveOffset);
                    if (headPos > 0)
                    {
                        response.SetHeaderRawData(_receiveHeaderBuffer, 0, headPos);
                        responseData = new byte[response.ContentLength];

                        int tailLength = receiveOffset - headPos - 4;

                        if (tailLength > 0)
                        {
                            Buffer.BlockCopy(_receiveHeaderBuffer, headPos + 4, responseData, 0, tailLength);
                            receiveOffset = tailLength;
                        }
                        else
                        {
                            receiveOffset = 0;
                        }

                        //set flag
                        findingHeader = false;
                    }
                    #endregion
                }
                else
                {
                    #region receive body
                    if (receiveOffset > responseData.Length)
                    {
                        throw new InvalidDataException("Response data overflow");
                    }

                    if (receiveOffset == responseData.Length)
                    {
                        break;
                    }

                    int recByte = _socket.Receive(
                        responseData,
                        receiveOffset,
                        responseData.Length - receiveOffset,
                        SocketFlags.None);

                    if (recByte == 0)
                    {
                        break;
                    }

                    receiveOffset += recByte;

                    #endregion
                }
            }// end while

            if (receiveOffset != responseData.Length)
            {
                throw new InvalidDataException("Invalid response data length");
            }

            return response;
        }

        private int seekSplitSymbolPosition(byte[] source, int startIndex, int count)
        {
            //the split-symbol is double "\r\n"
            byte[] splitSymbol = new byte[] { 0xd, 0xa, 0xd, 0xa };
            int endIndex = startIndex + count;
            for (int idx = startIndex; idx < endIndex; idx++)
            {
                int offset = 0;
                for (; offset < 4 && idx + offset < endIndex; offset++)
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

    } //end class
}
