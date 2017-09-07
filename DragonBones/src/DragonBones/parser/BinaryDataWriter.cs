using System.Text;
using System.IO;

namespace DragonBones
{
    public class BinaryDataWriter : BinaryWriter
    {
        public BinaryDataWriter() : base(new MemoryStream(0x100))
        {
        }

        internal BinaryDataWriter(Stream stream) : base(stream)
        {
        }

        public BinaryDataWriter(Encoding encoding) : base(new MemoryStream(0x100), encoding)
        {
        }

        internal BinaryDataWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public virtual void Write(bool[] value)
        {
            base.Write(value.Length);
            foreach (bool flag in value)
            {
                base.Write(flag);
            }
        }

        public override void Write(byte[] value)
        {
            base.Write(value.Length);
            foreach (byte num in value)
            {
                base.Write(num);
            }
        }

        public override void Write(char[] value)
        {
            base.Write(value.Length);
            foreach (char ch in value)
            {
                base.Write(ch);
            }
        }

        public virtual void Write(decimal[] value)
        {
            base.Write(value.Length);
            foreach (decimal num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(double[] value)
        {
            base.Write(value.Length);
            foreach (double num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(short[] value)
        {
            base.Write(value.Length);
            foreach (short num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(int[] value)
        {
            base.Write(value.Length);
            foreach (int num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(long[] value)
        {
            base.Write(value.Length);
            foreach (long num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(sbyte[] value)
        {
            base.Write(value.Length);
            foreach (sbyte num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(float[] value)
        {
            base.Write(value.Length);
            foreach (float num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(string[] value)
        {
            base.Write(value.Length);
            foreach (string str in value)
            {
                base.Write(str);
            }
        }

        public virtual void Write(ushort[] value)
        {
            base.Write(value.Length);
            foreach (ushort num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(uint[] value)
        {
            base.Write(value.Length);
            foreach (uint num in value)
            {
                base.Write(num);
            }
        }

        public virtual void Write(ulong[] value)
        {
            base.Write(value.Length);
            foreach (ulong num in value)
            {
                base.Write(num);
            }
        }

        private long Length
        {
            get
            {
                return this.BaseStream.Length;
            }
        }
    }

}
