using System;
using System.Collections.Generic;
using System.Text;
using Doms.TCClient.Configuration;

namespace Doms.TCClient
{
    class NonPersistentNodesList
    {
        public static string GetServerEndPoint(string key)
        {
            //find the right server node by the key
            FNV1a fnv = new FNV1a();
            byte[] hash = fnv.ComputeHash(Encoding.UTF8.GetBytes(key));
            int distNumber = (int)(BitConverter.ToUInt32(hash, 0) % 0xff);

            NonPersistentConfigElement rightNode = null;
            foreach (NonPersistentConfigElement node in TCClientConfigSection.Instance.NonPersistentNodes)
            {
                if (rightNode == null)
                {
                    rightNode = node;
                }

                if (distNumber <= node.Number)
                {
                    rightNode = node;
                    break;
                }
            }

            return rightNode.ToString(); ;
        }
    }
}
