using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// The presistance item info
    /// </summary>
    class PersistanceItemInfo
    {
         /// <summary>
        /// Item key
        /// </summary>
        public string Key { get; set; }
        
        /// <summary>
        /// The start position of data-record in the data file
        /// </summary>
        public long DataRecordPosition { get; set; }

        /// <summary>
        /// Content length
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// Last modified time of content
        /// </summary>
        public DateTime ItemTime { get; set; }

        /// <summary>
        /// Item data type and properties
        /// </summary>
        public int Properties { get; set; }
    }
}
