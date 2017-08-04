using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.TCClient.Client
{
    class ConnectionsOverflowException:Exception
    {
        private const string _message = "ThumbCached client network connections overflow.";

        public ConnectionsOverflowException() : base() { }

        public override string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
