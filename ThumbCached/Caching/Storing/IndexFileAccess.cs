using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Data.Common;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// Access the index database
    /// </summary>
    class IndexFileAccess
    {
        //the database connection
        private DbConnection _conn;

        public IndexFileAccess(string indexFile)
        {
            string connString =
                "Data Source="
                + indexFile
                + ";Synchronous=Off";

            //add "Synchronous=Off" to turn off the SQLite synchronous
            //that can improve record writting speed

            _conn = new SQLiteConnection(connString);
            _conn.Open();
        }

        public void Close()
        {
            _conn.Close();
        }

        /// <summary>
        /// Get the specify record
        /// </summary>
        public PersistanceItemInfo Get(string key)
        {
            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText =
                "select ItemKey,DataRecordPosition,ContentLength,LastModifyTime,Properties" +
                " from BlockInfo" +
                " where ItemKey=@ItemKey;";

            DbParameter para1 = cmd.CreateParameter();

            para1.ParameterName = "@ItemKey";
            para1.Value = key;

            cmd.Parameters.Add(para1);

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    PersistanceItemInfo item = new PersistanceItemInfo();
                    item.Key = reader.GetString(0);
                    item.DataRecordPosition = reader.GetInt64(1);
                    item.ContentLength = reader.GetInt32(2);
                    item.ItemTime = reader.GetDateTime(3);
                    item.Properties = reader.GetInt32(4);

                    return item;
                }
                else
                {
                    throw new StoringException("The specify item not found");
                }
            }

        }

        /// <summary>
        /// Check the specify record
        /// </summary>
        public bool Exist(string key)
        {

            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "select ItemKey" +
                " from BlockInfo" +
                " where ItemKey=@ItemKey;";

            DbParameter para1 = cmd.CreateParameter();

            para1.ParameterName = "@ItemKey";
            para1.Value = key;

            cmd.Parameters.Add(para1);

            bool hasRows = false;

            using (DbDataReader reader = cmd.ExecuteReader())
            {
                hasRows = reader.Read();
                reader.Close();
            }
            return hasRows;
        }

        /// <summary>
        /// Add a new record
        /// </summary>
        public void Add(PersistanceItemInfo item)
        {
            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "insert into BlockInfo" +
                " (ItemKey,DataRecordPosition,ContentLength,LastModifyTime,Properties)" +
                " values (@ItemKey,@DataRecordPosition,@ContentLength,@LastModifyTime,@Properties);";

            DbParameter para1 = cmd.CreateParameter();
            DbParameter para2 = cmd.CreateParameter();
            DbParameter para3 = cmd.CreateParameter();
            DbParameter para4 = cmd.CreateParameter();
            DbParameter para5 = cmd.CreateParameter();

            para1.ParameterName = "@ItemKey";
            para1.Value = item.Key;
            para2.ParameterName = "@DataRecordPosition";
            para2.Value = item.DataRecordPosition;
            para3.ParameterName = "@ContentLength";
            para3.Value = item.ContentLength;
            para4.ParameterName = "@LastModifyTime";
            para4.Value = item.ItemTime;
            para5.ParameterName = "@Properties";
            para5.Value = item.Properties;

            cmd.Parameters.Add(para1);
            cmd.Parameters.Add(para2);
            cmd.Parameters.Add(para3);
            cmd.Parameters.Add(para4);
            cmd.Parameters.Add(para5);

            cmd.ExecuteNonQuery();


        }

        /// <summary>
        /// Update the specify record
        /// </summary>
        public void Update(string key, int contentLength, DateTime lastModifiedTime, int properties)
        {
            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "update BlockInfo" +
                " set ContentLength = @ContentLength, LastModifyTime=@LastModifyTime, Properties=@Properties" +
                " where ItemKey=@ItemKey;";

            DbParameter para1 = cmd.CreateParameter();
            DbParameter para2 = cmd.CreateParameter();
            DbParameter para3 = cmd.CreateParameter();
            DbParameter para4 = cmd.CreateParameter();

            para1.ParameterName = "@ContentLength";
            para1.Value = contentLength;
            para2.ParameterName = "@LastModifyTime";
            para2.Value = lastModifiedTime;
            para3.ParameterName = "@Properties";
            para3.Value = properties;
            para4.ParameterName = "@ItemKey";
            para4.Value = key;

            cmd.Parameters.Add(para1);
            cmd.Parameters.Add(para2);
            cmd.Parameters.Add(para3);
            cmd.Parameters.Add(para4);

            cmd.ExecuteNonQuery();


        }

        /// <summary>
        /// Remove the specify record
        /// </summary>
        public void Remove(string key)
        {
            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = "delete from BlockInfo where ItemKey=@ItemKey;";

            DbParameter para1 = cmd.CreateParameter();

            para1.ParameterName = "@ItemKey";
            para1.Value = key;

            cmd.Parameters.Add(para1);

            cmd.ExecuteNonQuery();
        }

    }//end class
}
