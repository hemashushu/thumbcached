using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// The entity object for "BlockInfo" table in the database
    /// </summary>
    class InfoEntity
    {
         /// <summary>
        /// Block key
        /// </summary>
        public string ItemKey { get; set; }
        
        /// <summary>
        /// The start position of data-record in the blockdata.dat file
        /// </summary>
        public long DataRecordPosition { get; set; }

        /// <summary>
        /// The block binary content length
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// Last modify/update time of block
        /// </summary>
        public DateTime LastModifyTime { get; set; }

        /// <summary>
        /// Item properties
        /// </summary>
        public int Properties { get; set; }
    }
}
