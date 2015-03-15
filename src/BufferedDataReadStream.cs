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
    public class BufferedDataReadStream : Stream
    {
        private static LogSource Log = new LogSource("BufferedDataReadStream", LogSource.SourceLevel("BufferedDataReadStream"));

        private Stream stream;
        private MemoryStream memory;

        public BufferedDataReadStream(Stream stream)
        {
            this.stream = stream;
            this.memory = new MemoryStream();
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) created", this.GetHashCode());
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw Log.TraceException(new NotImplementedException());
        }

        public override long Length
        {
            get { throw Log.TraceException(new NotImplementedException()); }
        }

        public override long Position
        {
            get
            {
                return this.memory.Position;
            }
            set
            {
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) Read in buffer {1}, {2} bytes", this.GetHashCode(), buffer.GetHashCode(), count);
            AddToBuffer(count);

            return memory.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) Seek to offset: {1} seek origin {2}", this.GetHashCode(), offset, origin);
            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;

                case SeekOrigin.Current:
                    offset = offset + this.memory.Position;
                    break;

                default:
                    throw Log.TraceException(new NotSupportedException());
            }

            return this.memory.Seek(offset, SeekOrigin.Begin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private void AddToBuffer(long count)
        {
            long position = this.memory.Position;
            if (position + count > memory.Length)
            {
                if (position + count - this.memory.Length > Int32.MaxValue)
                {
                    throw Log.TraceException(new ArgumentOutOfRangeException());
                }

                int bytesToRead = (int)(position + count - this.memory.Length);
                byte[] data = new byte[bytesToRead];
                if (this.stream.Read(data, 0, bytesToRead) != bytesToRead)
                {
                    throw Log.TraceException(new EndOfStreamException());
                }

                this.memory.Seek(0, SeekOrigin.End);
                Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) Add {1} bytes to the buffer", this.GetHashCode(), data.Length);
                this.memory.Write(data, 0, data.Length);
                memory.Seek(position, SeekOrigin.Begin);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.memory != null)
            {
                this.memory.Dispose();
                this.memory = null;
            }
        }
    }
}
