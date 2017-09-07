using System.Text;
using System.IO;

namespace DragonBones
{
    public class BinaryDataReader : BinaryReader
    {
        private int i;
        private int readLength;

        internal BinaryDataReader(Stream stream) : base(stream)
        {
        }

        internal BinaryDataReader(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public virtual bool[] ReadBooleans()
        {
            this.readLength = base.ReadInt32();
            bool[] flagArray = new bool[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                flagArray[this.i] = base.ReadBoolean();
                this.i++;
            }
            return flagArray;
        }

        public virtual byte[] ReadBytes()
        {
            this.readLength = base.ReadInt32();
            byte[] buffer = new byte[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                buffer[this.i] = base.ReadByte();
                this.i++;
            }
            return buffer;
        }

        public virtual char[] ReadChars()
        {
            this.readLength = base.ReadInt32();
            char[] chArray = new char[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                chArray[this.i] = base.ReadChar();
                this.i++;
            }
            return chArray;
        }

        public virtual decimal[] ReadDecimals()
        {
            this.readLength = base.ReadInt32();
            decimal[] numArray = new decimal[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadDecimal();
                this.i++;
            }
            return numArray;
        }

        public virtual double[] ReadDoubles()
        {
            this.readLength = base.ReadInt32();
            double[] numArray = new double[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadDouble();
                this.i++;
            }
            return numArray;
        }

        public virtual short[] ReadInt16s()
        {
            this.readLength = base.ReadInt32();
            short[] numArray = new short[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadInt16();
                this.i++;
            }
            return numArray;
        }

        public virtual short[] ReadInt16s(int count)
        {

        }

        public virtual int[] ReadInt32s()
        {
            this.readLength = base.ReadInt32();
            int[] numArray = new int[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadInt32();
                this.i++;
            }
            return numArray;
        }

        public virtual long[] ReadInt64s()
        {
            this.readLength = base.ReadInt32();
            long[] numArray = new long[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadInt64();
                this.i++;
            }
            return numArray;
        }

        public virtual sbyte[] ReadSBytes()
        {
            this.readLength = base.ReadInt32();
            sbyte[] numArray = new sbyte[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadSByte();
                this.i++;
            }
            return numArray;
        }

        public virtual float[] ReadSingles()
        {
            this.readLength = base.ReadInt32();
            float[] numArray = new float[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadSingle();
                this.i++;
            }
            return numArray;
        }

        public virtual string[] ReadStrings()
        {
            this.readLength = base.ReadInt32();
            string[] strArray = new string[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                strArray[this.i] = base.ReadString();
                this.i++;
            }
            return strArray;
        }

        public virtual ushort[] ReadUInt16s()
        {
            this.readLength = base.ReadInt32();
            ushort[] numArray = new ushort[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadUInt16();
                this.i++;
            }
            return numArray;
        }

        public virtual uint[] ReadUInt32s()
        {
            this.readLength = base.ReadInt32();
            uint[] numArray = new uint[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadUInt32();
                this.i++;
            }
            return numArray;
        }

        public virtual ulong[] ReadUInt64s()
        {
            this.readLength = base.ReadInt32();
            ulong[] numArray = new ulong[this.readLength];
            this.i = 0;
            while (this.i < this.readLength)
            {
                numArray[this.i] = base.ReadUInt64();
                this.i++;
            }
            return numArray;
        }

        private long Length
        {
            get
            {
                return this.BaseStream.Length;
            }
        }

        /// <summary>
        /// 基础值类型数组解析
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <param name="bytes">字节流数组</param>
        /// <param name="index">开始Index</param>
        /// <param name="len">数组长度</param>
        /// <returns></returns>
        private static T[] GetValues<T>(byte[] bytes, ref int index, int len) where T : struct
        {
            T[] bc = new T[len];
            int i = 0;
            switch (typeof(T).Name.ToLower())
            {
                case "uint16":
                case "ushort":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt16(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "int16":
                case "short":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt16(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
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
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "int64":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt64(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "uint64":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt64(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "byte":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(bytes[index], typeof(T));
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "int32":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToInt32(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
                            i += 1;
                        }
                    }
                    break;
                case "uint32":
                    {
                        while (i < len)
                        {
                            bc[i] = (T)Convert.ChangeType(BitConverter.ToUInt32(bytes, index), typeof(T));
                            index += Marshal.SizeOf(typeof(T));
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
    }

}
