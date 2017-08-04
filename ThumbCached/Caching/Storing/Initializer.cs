using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// Check and rebuild index database and binary data file if need
    /// </summary>
    class Initializer
    {
        public static void Check(string infoFile, string dataFile)
        {
            //rebuild the files if they does not exist

            if (!File.Exists(infoFile))
            {
                RebuildIndexFile(infoFile);
            }

            if (!File.Exists(dataFile))
            {
                RebuildDataFile(dataFile);
            }
        }

        /// <summary>
        /// Rebuild index database
        /// </summary>
        private static void RebuildIndexFile(string indexFile)
        {

            //open database
            DbConnection conn = new SQLiteConnection(
                "Data Source=" + indexFile);
            conn.Open();
            DbCommand cmd = conn.CreateCommand();

            //NOTE::
            //the normal sql string is like this:
            //
            //create table BlockInfo(
            //    ItemKey varchar(32),
            //    DataRecordPosition bigint,
            //    ContentLength int,
            //    LastModifyTime datetime,
            //    Properties int);
            //
            //the following is for SQLite specially

            //create table
            cmd.CommandText = "create table BlockInfo (" +
                "ItemKey TEXT, " +
                "DataRecordPosition INTEGER, " +
                "ContentLength INTEGER, " +
                "LastModifyTime TEXT, " +
                "Properties INTEGER);";
            cmd.ExecuteNonQuery();

            //create index
            cmd.CommandText = "create UNIQUE index IND_blockinfo on BlockInfo (ItemKey)";
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        /// <summary>
        /// Rebuild binary data file
        /// </summary>
        private static void RebuildDataFile(string dataFile)
        {
            FileStream stream = new FileStream(
                dataFile, FileMode.Create, FileAccess.Write);

            //the file header total length is 512 byte
            byte[] header = new byte[512];

            string headerName = "KWY/TCD";
            int headerVersion = 10;
            long headerAllRecordsLength = header.Length;

            BitConverterEx.SetBytes(headerName, header, 0, 16, Encoding.ASCII); //header segment: Name offset=0
            BitConverterEx.SetBytes(headerVersion, header, 16); //header segment: Version offset=16
            BitConverterEx.SetBytes(headerAllRecordsLength, header, 20); //header segment: AllRecordsLength offset=20
            stream.Write(header, 0, header.Length);
            stream.Close();
        }

    }//end class
}
