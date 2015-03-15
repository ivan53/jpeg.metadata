using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMetadata
{
    /// <summary>
    /// Binary reader that supports big endian and little endian
    /// </summary>
    public class DataReader : IDisposable
    {
        private static LogSource Log = new LogSource("DataReader", LogSource.SourceLevel("DataReader"));

        private Stream stream;
        private bool littleEndian;

        public DataReader(Stream stream)
        {
            this.LittleEndian = true;
            this.stream = stream;
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) created", this.GetHashCode());
        }

        public bool LittleEndian 
        {
            get
            {
                return this.littleEndian;
            }

            set
            {
                Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) little endian set to: {1}", this.GetHashCode(), value);
                this.littleEndian = value;
            }
        }

        public long Position
        {
            get
            {
                return this.stream.Position;
            }
        }

        public byte ReadByte()
        {
            int data = this.stream.ReadByte();
            if (data == -1)
            {
                throw Log.TraceException(new EndOfStreamException());
            }

            return (byte)data;
        }

        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            int result = this.stream.Read(data, 0, data.Length);
            if (result != data.Length)
            {
                throw Log.TraceException(new EndOfStreamException());
            }

            return data;
        }

        public UInt16 ReadUInt16()
        {
            UInt16 result = (UInt16)this.ReadAndConvert(sizeof(UInt16));
            return result;
        }

        public Int32 ReadInt32()
        {
            Int32 result = (Int32)this.ReadAndConvert(sizeof(Int32));
            return result;
        }

        public UInt32 ReadUInt32()
        {
            UInt32 result = (UInt32)this.ReadAndConvert(sizeof(UInt32));
            return result;
        }

        public char[] ReadChars(int count, Encoding encoding)
        {
            byte[] data = this.ReadBytes(count);
            char[] result = encoding.GetChars(data);
            return result;
        }

        public void Seek(long offset)
        {
            this.stream.Seek(offset, SeekOrigin.Begin);
        }

        private UInt64 ReadAndConvert(int bytes)
        {
            byte[] data = this.ReadBytes(bytes);

            UInt64 result = 0;
            if (this.LittleEndian)
            {
                UInt64 multiplier = 1;
                foreach (var d in data)
                {
                    result = result + d * multiplier;
                    multiplier *= 256;
                }
            }
            else
            {
                foreach (var d in data)
                {
                    result = result * 256 + d;
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }
        }
    }
}
