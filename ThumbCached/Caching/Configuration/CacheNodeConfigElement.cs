using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.ThumbCached.Configuration
{
    public class CacheNodeConfigElement:ConfigurationElement
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string NodeId
        {
            get { return (string)this["id"]; }
            set { this["id"] = value; }
        }

        [ConfigurationProperty("storing", IsRequired = false)]
        public StoringConfigElement Storing
        {
            get { return (StoringConfigElement)this["storing"]; }
            set { this["storing"] = value; }
        }

        /// <summary>
        /// The size of cache pool
        /// </summary>
        [ConfigurationProperty("poolSize", IsRequired = true, DefaultValue = 64)]
        [IntegerValidator(MinValue = 0, MaxValue = 2048)]
        public int PoolSize
        {
            get { return (int)this["poolSize"]; }
            set { this["poolSize"] = value; }
        }

        /// <summary>
        /// Enable persistance or not
        /// </summary>
        [ConfigurationProperty("persistEnable", DefaultValue = true)]
        public bool PersistEnable
        {
            get { return (bool)this["persistEnable"]; }
            set { this["persistEnable"] = value; }
        }

    }
}
