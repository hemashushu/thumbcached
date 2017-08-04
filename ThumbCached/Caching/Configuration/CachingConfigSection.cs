using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.ThumbCached.Configuration
{
    public class CachingConfigSection:ConfigurationSection
    {

        #region Instance
        private static CachingConfigSection _instance =
            (CachingConfigSection)ConfigurationManager.GetSection("caching");

        public static CachingConfigSection Instance
        {
            get { return _instance; }
        }
        #endregion

        [ConfigurationProperty("cache", IsRequired = true)]
        [ConfigurationCollection(typeof(CacheNodeConfigElementCollection),
            AddItemName = "node")]
        public CacheNodeConfigElementCollection CacheNodes
        {
            get { return (CacheNodeConfigElementCollection)this["cache"]; }
            set { this["cache"] = value; }
        }

    }
}
