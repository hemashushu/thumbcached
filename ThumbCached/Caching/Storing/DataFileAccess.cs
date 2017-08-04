using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// Access the binary data file
    /// </summary>
    class DataFileAccess
    {

        //Note:: The sturcture of file
        //-------------------------------------------------------------------------------------------------------------------
        // FILE = FILE HEADER + DATA RECORD(s) + RESERVE SPACE
        // FILE HEADER = 
        //                  NAME (16 bytes) + 
        //                  VERSION (4 bytes) + 
        //                  ALL RECORDS LENGTH (8 bytes) + 
        //                  HEADER SPACE (484 bytes)
        // DATA RECORD = 
        //                  RECORD HEADER MARK (4 bytes) +
        //                  DATA AREA LENGTH (4 bytes) + 
        //                  CONTENT LENGTH (4 bytes) + 
        //                  NEXT LINK START POSITION (8 byte) + 
        //                  CONTENT ( x bytes)
        //
        //NOTE:: the data record size is multiple of _PageSize;

        #region private varables

        //object for synchronous while reading/writing data-recored
        private object _syncObject;

        //all data-records total length, include file header lenth,
        //this variable is used to record the actually effective data in the binary file
        private long _allRecordsLength;

        //the file stream
        private FileStream _fileStream;

        //record header length, include: record-start-mark, data area length value, 
        //content length value, the next link record start position
        private const int _RecordHeaderLength = 20;

        //Page is the min size of a data record length
        private const int _PageSize = 128;

        //the reserve space size
        //reserve space can improve the speed while appending new records
        private const int _ReserveSpaceSize = _PageSize * 8 * 1024;

        //file header length
        private const int _FileHeaderLength = 512;

        //logger
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        public DataFileAccess(string dataFile)
        {
            _syncObject = new object();
            _fileStream = new FileStream(
                dataFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            readFileHeaderInfo();
        }

        public void Close()
        {
            _fileStream.Close();
        }

        #region file/record header parser

        private void readFileHeaderInfo()
        {
            byte[] data = new byte[8];

            //get the "allRecordsLength" value
            _fileStream.Seek(20, SeekOrigin.Begin);
            _fileStream.Read(data, 0, 8);
            _allRecordsLength = BitConverter.ToInt64(data, 0);
        }

        private void updateFileHeaderInfo()
        {
            byte[] data = new byte[8];

            //save the "allRecordsLength" value
            BitConverterEx.SetBytes(_allRecordsLength, data, 0);
            _fileStream.Seek(20, SeekOrigin.Begin);
            _fileStream.Write(data, 0, 8);
            _fileStream.Flush();
        }

        private void parseRecordHeader(byte[] header,
            out int dataAreaLength, out int contentLength, out long nextRecordStartPosition,
            long headerPosition)
        {
            //check the record header mark
            for (int idx = 0; idx < 4; idx++)
            {
                if (header[idx] != 0xFF)
                {
                    throw new StoringException("Invalid data record header mark");
                }
            }

            //get each segment value
            dataAreaLength = BitConverter.ToInt32(header, 4);
            contentLength = BitConverter.ToInt32(header, 8);
            nextRecordStartPosition = BitConverter.ToInt64(header, 12);

            //check the link position whether valid or not
            if ((nextRecordStartPosition > 0 && nextRecordStartPosition <= headerPosition) ||
                nextRecordStartPosition > _allRecordsLength)
            {
                logger.Warn("Invalid link record start position: " + nextRecordStartPosition + ", current pos: " + headerPosition);
                throw new StoringException("Invalid link record start position");
            }

            if (dataAreaLength <= 0)
            {
                throw new StoringException("Invalid data record content area length");
            }
        }

        private void combineRecordHeader(byte[] header,
            int dataAreaLength, int contentLength, long nextRecordStartPosition)
        {
            //record header mark
            for (int idx = 0; idx < 4; idx++)
            {
                header[idx] = 0xff;
            }

            BitConverterEx.SetBytes(dataAreaLength, header, 4);
            BitConverterEx.SetBytes(contentLength, header, 8);
            BitConverterEx.SetBytes(nextRecordStartPosition, header, 12);
        }
        #endregion

        #region get

        /// <summary>
        /// Read record content
        /// </summary>
        public void Get(long recordStartPosition, byte[] data, int length)
        {
            lock (_syncObject)
            {

                if (recordStartPosition < _FileHeaderLength || recordStartPosition > _allRecordsLength)
                {
                    throw new InvalidOperationException("Invalid record start position");
                }

                if (length <= 0)
                {
                    throw new ArgumentException("The length must greate than 0");
                }

                int totalReadBytes = 0;

                while (recordStartPosition > 0 && totalReadBytes < length)
                {
                    byte[] header = new byte[_RecordHeaderLength]; //record header

                    //read the current record header
                    _fileStream.Seek(recordStartPosition, SeekOrigin.Begin);
                    int readByte = _fileStream.Read(header, 0, header.Length);

                    //get header infomation
                    int dataAreaLength;
                    int contentLength;
                    long nextLinkPosition;

                    parseRecordHeader(
                        header,
                        out dataAreaLength, out contentLength, out nextLinkPosition,
                        recordStartPosition);

                    //read the content data
                    readByte = _fileStream.Read(data, totalReadBytes, contentLength);
                    totalReadBytes += readByte;

                    //point to next link record position
                    recordStartPosition = nextLinkPosition;
                }

                if (totalReadBytes != length)
                {
                    throw new StoringException("Data record length error");
                }
            }
        }
        #endregion

        #region update

        /// <summary>
        /// Update the exist record content
        /// </summary>
        public void Update(long recordStartPosition, byte[] data, int length)
        {

            lock (_syncObject)
            {
                if (recordStartPosition < _FileHeaderLength || recordStartPosition > _allRecordsLength)
                {
                    throw new InvalidOperationException("Invalid record start position");
                }

                if (length <= 0)
                {
                    throw new ArgumentException("The length must greate than 0");
                }

                int totalWroteBytes = 0;

                while (length - totalWroteBytes > 0) //there is remain content yet
                {
                    // the old record and it's all "link-records"
                    #region update

                    long currentRecordPosition = recordStartPosition;

                    byte[] header = new byte[_RecordHeaderLength]; //record header

                    //read old record header
                    _fileStream.Seek(currentRecordPosition, SeekOrigin.Begin);
                    int readByte = _fileStream.Read(header, 0, header.Length);

                    //get header infomation
                    int dataAreaLength;
                    int contentLength;
                    long nextLinkPosition;

                    parseRecordHeader(
                        header,
                        out dataAreaLength, out contentLength, out nextLinkPosition,
                        currentRecordPosition);

                    //calculate bytes that can be wrote at this time
                    int writeBytes = length - totalWroteBytes;
                    if (writeBytes > dataAreaLength)
                    {
                        writeBytes = dataAreaLength;
                    }

                    //overwrite the old data area
                    //NOTE:: can not bypass this line in MONO, or it will be error
                    _fileStream.Seek(currentRecordPosition + _RecordHeaderLength, SeekOrigin.Begin);
                    _fileStream.Write(data, totalWroteBytes, writeBytes);
                    _fileStream.Flush();

                    //calculate the remain content
                    totalWroteBytes += writeBytes;

                    //handle the remain data
                    if (length - totalWroteBytes > 0)
                    {
                        if (nextLinkPosition > 0)
                        {
                            //redirect to the next link record
                            recordStartPosition = nextLinkPosition;
                        }
                        else
                        {
                            //need create new "link record"
                            nextLinkPosition = addRecord(data, totalWroteBytes, length - totalWroteBytes);
                            totalWroteBytes = length;
                        }
                    }

                    //re-combine record header
                    combineRecordHeader(header, dataAreaLength, writeBytes, nextLinkPosition);

                    //update the current record header
                    _fileStream.Seek(currentRecordPosition, SeekOrigin.Begin);
                    _fileStream.Write(header, 0, header.Length);
                    _fileStream.Flush();
                    #endregion

                }//end while
            }
        }
        #endregion

        #region add

        /// <summary>
        /// Add new record
        /// </summary>
        public long Add(byte[] data)
        {
            lock (_syncObject)
            {
                return addRecord(data, 0, data.Length);
            }
        }

        private long addRecord(byte[] data, int offset, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("The length must large than 0");
            }

            //increase file free space
            long needExtendSize = _allRecordsLength + length - _fileStream.Length;
            if (needExtendSize >= 0)
            {
                extendFileSpace((int)needExtendSize);
            }

            //assign new record length to _PageSize multiple
            int dataAreaLength = length;
            if ((dataAreaLength + _RecordHeaderLength) % _PageSize > 0)
            {
                dataAreaLength = (((dataAreaLength + _RecordHeaderLength) / _PageSize) + 1)
                    * _PageSize - _RecordHeaderLength;
            }

            //allocation new record start position to _PageSize multiple
            long newRecordPosition = _allRecordsLength;
            if (newRecordPosition % _PageSize > 0)
            {
                long correctPos = ((newRecordPosition / _PageSize) + 1) * (long)_PageSize;
                logger.Warn("Invalid new record start position:" + newRecordPosition + ", correct to:" + correctPos);
                newRecordPosition = correctPos;
            }

            //write record header
            byte[] header = new byte[_RecordHeaderLength];
            combineRecordHeader(header, dataAreaLength, length, 0);

            //seek to the end of last record
            _fileStream.Seek(newRecordPosition, SeekOrigin.Begin);
            _fileStream.Write(header, 0, header.Length); //write record header
            _fileStream.Write(data, offset, length); //write record content
            _fileStream.Flush();

            //update file header info
            _allRecordsLength = newRecordPosition + _RecordHeaderLength + dataAreaLength;

            updateFileHeaderInfo();

            //return the new record start up position
            return newRecordPosition;
        }
        #endregion

        #region functions
        private void extendFileSpace(int extSize)
        {
            if (extSize <= _ReserveSpaceSize)
            {
                extSize = _ReserveSpaceSize;
            }
            else
            {
                extSize = ((extSize / _ReserveSpaceSize) + 1) * _ReserveSpaceSize; //factor
            }

            _fileStream.Seek(_allRecordsLength, SeekOrigin.Begin);

            byte[] buffer = new byte[16 * 1024];
            for (int idx = 0; idx < extSize / buffer.Length; idx++)
            {
                _fileStream.Write(buffer, 0, buffer.Length);
            }
            _fileStream.Flush();
        }

        #endregion
    }
}
