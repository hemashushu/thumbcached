using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.TCClient.Configuration
{
    class ConnectionConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("maxconn", IsRequired = false, DefaultValue = 20)]
        [IntegerValidator(MinValue = 2, MaxValue = 256)]
        public int MaxConnections
        {
            get { return (int)base["maxconn"]; }
            set { base["maxconn"] = value; }
        }

        [ConfigurationProperty("timeout", IsRequired = false, DefaultValue = 5)]
        [IntegerValidator(MinValue = 2, MaxValue = 30)]
        public int Timeout
        {
            get { return (int)base["timeout"]; }
            set { base["timeout"] = value; }
        }
       
    }
}
