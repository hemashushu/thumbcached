using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.ThumbCached.Configuration
{
    public class CacheNodeConfigElementCollection:ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CacheNodeConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CacheNodeConfigElement)element).NodeId;
        }

        public void Add(CacheNodeConfigElement element)
        {
            base.BaseAdd(element);
        }
    }
}
