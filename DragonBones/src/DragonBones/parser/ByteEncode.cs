//=====================================================================
//
//  All right reserved
//  filename : BytesDecode
//  description :
//
//  create by User at 2016/8/12 9:57:15
//=====================================================================
using System;
using System.Runtime.InteropServices;

namespace BytesLib.com
{
    /// <summary>
    /// 解析字节流
    /// </summary>
    internal sealed class BytesDecode
    {
        /// <summary>
        /// 解析基础值类型
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <returns></returns>
        public T getStructValue<T>(byte[] bytes, ref int index) where T : struct
        {
            return this.getValue<T>(bytes, ref index);
        }
        /// <summary>
        /// 解析基础值类型数组
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <param name="len">数组长度</param>
        /// <returns></returns>
        public T[] getStructValus<T>(byte[] bytes, ref int index, int len) where T : struct
        {
            return this.getValues<T>(bytes, ref index, len);
        }
        /// <summary>
        /// 基础值类型2维数组解析
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <param name="lenD1">一维Length</param>
        /// <param name="lenD2">二维Length</param>
        /// <returns></returns>
        public T[,] getStructValus2D<T>(byte[] bytes, ref int index, int lenD1, int lenD2) where T : struct
        {
            return this.getValues2D<T>(bytes, ref index, lenD1, lenD2);
        }
        #region
        /// <summary>
        /// 基础值类型解析
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <returns></returns>
        private T getValue<T>(byte[] bytes, ref int index) where T : struct
        {
            T bc = default(T);
            switch (typeof(T).Name.ToLower())
            {
                case "uint16":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToUInt16(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "int16":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToInt16(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "bool":
                case "boolean":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToBoolean(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "int64":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToInt64(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "uint64":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToUInt64(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "byte":
                    {
                        bc = (T)Convert.ChangeType(bytes[index], typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "int32":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToInt32(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
                case "uint32":
                    {
                        bc = (T)Convert.ChangeType(BitConverter.ToUInt32(bytes, index), typeof(T));
                        index += Marshal.SizeOf<T>();
                    }
                    break;
            }
            return bc;
        }
        /// <summary>
        /// 基础值类型数组解析
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <param name="len">数组长度</param>
        /// <returns></returns>
        private T[] getValues<T>(byte[] bytes, ref int index, int len) where T : struct
        {
            T[] bc = new T[len];
            int i = 0;
            switch (typeof(T).Name.ToLower())
            {
                case "uint16":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt16(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "int16":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt16(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "bool":
                case "boolean":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToBoolean(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "int64":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt64(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "uint64":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt64(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "byte":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(bytes[index], typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "int32":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt32(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "uint32":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt32(bytes, index), typeof(T));
                            index += Marshal.SizeOf<T>();
                            i += 1;
                        }
                    }
                    break;
                case "char":
                    {
                        Buffer.BlockCopy(bytes, index, bc, 0, len);
                        index += Marshal.SizeOf(bc);
                    }
                    break;
            }
            return bc;
        }
        /// <summary>
        /// 基础值类型2维数组解析
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <param name="lenD1">一维Length</param>
        /// <param name="lenD2">二维Length</param>
        /// <returns></returns>
        private T[,] getValues2D<T>(byte[] bytes, ref int index, int lenD1, int lenD2) where T : struct
        {
            T[,] bc = new T[lenD1, lenD2];
            int i = 0;
            switch (typeof(T).Name.ToLower())
            {
                case "uint16":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToUInt16(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "int16":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToInt16(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "bool":
                case "boolean":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToBoolean(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "int64":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToInt64(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "uint64":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToUInt64(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "byte":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(bytes[index], typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "int32":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToInt32(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "uint32":
                    {
                        while (i < lenD1)
                        {
                            for (int j = 0; j < lenD2; j += 1)
                            {
                                bc[i, j] = (T)Convert.ChangeType(BitConverter.ToUInt32(bytes, index), typeof(T));
                                index += Marshal.SizeOf<T>();
                            }
                            i += 1;
                        }
                    }
                    break;
                case "char":
                    {
                        Buffer.BlockCopy(bytes, index, bc, 0, lenD1 * lenD2);
                        index += Marshal.SizeOf(bc);
                    }
                    break;
            }
            return bc;
        }
        #endregion
    }
}