using System;
using System.Collections.Generic;
using System.Text;

namespace Doms.ThumbCached.Caching.Storing
{
    /// <summary>
    /// An extension for System.BitConverter
    /// </summary>
    class BitConverterEx
    {
        //NOTE::
        //when convert integer into a byte array, use the "LittleEndian" (x86 cpu) order, i.e.
        //interger 0x1234 will convert into: 0x34,0x12,0x00,0x00

        public static readonly bool IsLittleEndian = true;

        /// <summary>
        /// Set Boolean value into a byte array
        /// </summary>
        public static void SetBytes(bool val, byte[] data, int startIndex)
        {
            data[startIndex] = (byte)(val ? 1 : 0);
        }

        /// <summary>
        /// Set 16-bit integer value into a byte array
        /// </summary>
        public static void SetBytes(short val, byte[] data, int startIndex)
        {
            for (int i = 0; i < 2; i++)
            {
                data[startIndex + i] = (byte)((val >> (8 * i)) & 0xff);
            }
        }

        /// <summary>
        /// Set 32-bit integer value into a byte array
        /// </summary>
        public static void SetBytes(int val, byte[] data, int startIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                data[startIndex + i] = (byte)((val >> (8 * i)) & 0xff);
            }
        }

        /// <summary>
        /// Set 64-bit integer value into a byte array
        /// </summary>
        public static void SetBytes(long val, byte[] data, int startIndex)
        {
            for (int i = 0; i < 8; i++)
            {
                data[startIndex + i] = (byte)((val >> (8 * i)) & 0xff);
            }
        }

        /// <summary>
        /// Set a string into a byte array
        /// </summary>
        public static void SetBytes(
            string val, byte[] data, int startIndex, int areaLength, Encoding encoding)
        {
            if (string.IsNullOrEmpty(val)) return;

            byte[] tmp = encoding.GetBytes(val);
            int copylength = Math.Min(tmp.Length, areaLength);
            if (copylength + startIndex > data.Length)
            {
                throw new OverflowException("Source string is too long");
            }

            Buffer.BlockCopy(tmp, 0, data, startIndex, copylength);
        }

        /// <summary>
        /// Convert byte array to string
        /// </summary>
        public static string ToString(
            byte[] data, int startIndex, int areaLength, Encoding encoding)
        {
            //trim the chars after '\0'
            int pos = Array.IndexOf<byte>(data, 0, startIndex, areaLength);
            int copylength = (pos < 0) ? areaLength : pos - startIndex;
            return encoding.GetString(data, startIndex, copylength);
        }

        /// <summary>
        /// Convert a byte array to hex string
        /// </summary>
        public static string ToHexString(byte[] data, int startIndex, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int idx = startIndex; idx < startIndex + length; idx++)
            {
                sb.Append(data[idx].ToString("x2"));
            }
            return sb.ToString();
        }

    }//end class
}
