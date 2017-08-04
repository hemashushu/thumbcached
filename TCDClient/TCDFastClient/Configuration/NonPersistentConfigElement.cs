using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.TCClient.Configuration
{
    class NonPersistentConfigElement:ConfigurationElement
    {
        [ConfigurationProperty("number", IsRequired = true, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0, MaxValue = 255)]
        public int Number
        {
            get { return (int)base["number"]; }
            set { base["number"] = value; }
        }

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get { return (string)base["address"]; }
            set { base["address"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true, DefaultValue = 18600)]
        [IntegerValidator(MinValue = 1, MaxValue = 65535)]
        public int Port
        {
            get { return (int)base["port"]; }
            set { base["port"] = value; }
        }

        public override string ToString()
        {
            return this.Address + ":" + this.Port;
        }

        public System.Net.IPEndPoint ToEndPoint()
        {
            return new System.Net.IPEndPoint(
                System.Net.IPAddress.Parse(this.Address), this.Port);
        }
    }
}
