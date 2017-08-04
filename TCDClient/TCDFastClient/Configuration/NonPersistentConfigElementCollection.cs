using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Doms.TCClient.Configuration
{
    class NonPersistentConfigElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NonPersistentConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NonPersistentConfigElement)element).Number;
        }

        public NonPersistentConfigElement GetServerNode(int serverNumber)
        {
            return (NonPersistentConfigElement)base.BaseGet(serverNumber);
        }

        public void Add(NonPersistentConfigElement element)
        {
            base.BaseAdd(element);
        }
    }
}
