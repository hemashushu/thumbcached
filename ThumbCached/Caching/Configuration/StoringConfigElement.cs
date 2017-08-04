using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.ThumbCached.Configuration
{
    public class StoringConfigElement:ConfigurationElement
    {
        [ConfigurationProperty("indexFile", DefaultValue = "cacheindex.db", IsRequired = true)]
        public string IndexFile
        {
            get { return (string)this["indexFile"]; }
            set { this["indexFile"] = value; }
        }

        [ConfigurationProperty("dataFile", DefaultValue = "cachedata.dat", IsRequired = true)]
        public string DataFile
        {
            get { return (string)this["dataFile"]; }
            set { this["dataFile"] = value; }
        }

    }
}
