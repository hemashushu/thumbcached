using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Doms.ThumbCached.Embed
{
    class DataSerialize
    {
        private static readonly byte[] _emptyValue = new byte[1] { 0x0 };

        public static void Serialize(object value, out byte[] data, out CacheDataType dataType)
        {

            if (value == null)
            {
                throw new ArgumentNullException("value can not be null");
            }

            byte[] test = value as byte[];
            if (test != null)
            {
                if (test.Length == 0)
                {
                    data = _emptyValue;
                    dataType = CacheDataType.EmptyByteArray;
                }
                else
                {
                    data = test;
                    dataType = CacheDataType.Binary;
                }
                return;
            }

            TypeCode typecode = Type.GetTypeCode(value.GetType());
            switch (typecode)
            {
                case TypeCode.Object:
                    using (MemoryStream stream = new MemoryStream())
                    {
                        BinaryFormatter ser = new BinaryFormatter();
                        ser.Serialize(stream, value);
                        data = stream.ToArray();
                        dataType = CacheDataType.Object;
                    }
                    break;

                case TypeCode.Boolean:
                    data = BitConverter.GetBytes((bool)value);
                    dataType = CacheDataType.Boolean;
                    break;

                case TypeCode.Int32:
                    data = BitConverter.GetBytes((int)value);
                    dataType = CacheDataType.Int32;
                    break;

                case TypeCode.Int64:
                    data = BitConverter.GetBytes((long)value);
                    dataType = CacheDataType.Int64;
                    break;

                case TypeCode.Single:
                    data = BitConverter.GetBytes((float)value);
                    dataType = CacheDataType.Single;
                    break;

                case TypeCode.Double:
                    data = BitConverter.GetBytes((double)value);
                    dataType = CacheDataType.Double;
                    break;

                case TypeCode.DateTime:
                    data = BitConverter.GetBytes(((DateTime)value).ToBinary());
                    dataType = CacheDataType.DateTime;
                    break;

                case TypeCode.String:
                    data = Encoding.UTF8.GetBytes((string)value);
                    if (data.Length == 0)
                    {
                        data = _emptyValue;
                        dataType = CacheDataType.EmptyString;
                    }
                    else
                    {
                        dataType = CacheDataType.String;
                    }
                    break;

                default:
                    throw new NotSupportedException("Not supported data type");
            }
        }

        public static object Deserialize(byte[] data, CacheDataType dataType)
        {

            if (data == null)
            {
                throw new ArgumentNullException("data cannot be null");
            }

            if (dataType == CacheDataType.Binary)
            {
                return data;
            }

            if (dataType == CacheDataType.EmptyByteArray)
            {
                return new byte[0];
            }

            if (dataType == CacheDataType.EmptyString)
            {
                return String.Empty;
            }

            if (dataType == CacheDataType.Object)
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    BinaryFormatter ser = new BinaryFormatter();
                    return ser.Deserialize(stream);
                }
            }

            switch (dataType)
            {
                case CacheDataType.Boolean:
                    return BitConverter.ToBoolean(data, 0);

                case CacheDataType.Int32:
                    return BitConverter.ToInt32(data, 0);

                case CacheDataType.Int64:
                    return BitConverter.ToInt64(data, 0);

                case CacheDataType.Single:
                    return BitConverter.ToSingle(data, 0);

                case CacheDataType.Double:
                    return BitConverter.ToDouble(data, 0);

                case CacheDataType.DateTime:
                    return DateTime.FromBinary(BitConverter.ToInt64(data, 0));

                case CacheDataType.String:
                    return Encoding.UTF8.GetString(data);

                default:
                    throw new NotSupportedException("Not supported data type");
            }
        }

    }
}
