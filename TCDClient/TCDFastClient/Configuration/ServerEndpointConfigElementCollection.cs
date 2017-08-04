using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.TCClient.Configuration
{
    class ServerEndpointConfigElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerEndpointConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerEndpointConfigElement)element).Name;
        }

         public ServerEndpointConfigElement GetServerNode(string serverId)
        {
            return (ServerEndpointConfigElement)base.BaseGet(serverId);
        }

        public void Add(ServerEndpointConfigElement element)
        {
            base.BaseAdd(element);
        }
    }
}
