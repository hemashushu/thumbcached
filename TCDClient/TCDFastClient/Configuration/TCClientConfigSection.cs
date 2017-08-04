using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.TCClient.Configuration
{
    class TCClientConfigSection : ConfigurationSection
    {
        #region Instance
        private static TCClientConfigSection _instance =
            (TCClientConfigSection)ConfigurationManager.GetSection("tcdClient");

        public static TCClientConfigSection Instance
        {
            get { return _instance; }
        }
        #endregion

        [ConfigurationProperty("serverEndpoints",IsRequired=true)]
        [ConfigurationCollection(typeof(ServerEndpointConfigElementCollection),
           AddItemName = "endpoint")]
        public ServerEndpointConfigElementCollection ServerNodes
        {
            get { return (ServerEndpointConfigElementCollection)this["serverEndpoints"]; }
            set { this["serverEndpoints"] = value; }
        }

        [ConfigurationProperty("nonPersistent", IsRequired = false)]
        [ConfigurationCollection(typeof(NonPersistentConfigElementCollection),
           AddItemName = "node")]
        public NonPersistentConfigElementCollection NonPersistentNodes
        {
            get { return (NonPersistentConfigElementCollection)this["nonPersistent"]; }
            set { this["nonPersistent"] = value; }
        }

        [ConfigurationProperty("connection", IsRequired = false)]
        public ConnectionConfigElement Connection
        {
            get { return (ConnectionConfigElement)this["connection"]; }
            set { this["connection"] = value; }
        }

    }
}
