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
    /// Finds and deserializes a tiff header of an image file.
    /// </summary>
    public class TiffHeader
    {
        private static LogSource Log = new LogSource("TiffHeader", LogSource.SourceLevel("TiffHeader"));

        private List<ImageFileDirectory> ifdList;

        public static TiffHeader Deserialize(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    var tiffHeader = TiffHeader.Deserialize(stream);
                    return tiffHeader;
                }
                catch (InvalidDataException)
                {
                    Log.TraceEvent(TraceEventType.Warning, 0, "Failed to process file: {0}", fileName);
                    return null;
                }
            }
        }

        public static TiffHeader Deserialize(Stream stream)
        {
            var tiffHeader = new TiffHeader();
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) Deserialize from stream {1}", tiffHeader.GetHashCode(), stream.GetHashCode());

            if (!FindBeginning(stream)) return null;

            using (var reader = new DataReader(new BufferedDataReadStream(stream)))
            {
                var byteOrder = new string(reader.ReadChars(2, Encoding.ASCII));

                if (byteOrder == "II")
                {
                    reader.LittleEndian = true;
                }
                else if (byteOrder == "MM")
                {
                    reader.LittleEndian = false;
                }
                else
                {
                    throw new InvalidDataException(string.Format("The data format has unknown byte order {0}", byteOrder));
                }

                UInt16 fourtyTwo = reader.ReadUInt16();
                if (fourtyTwo != 42)
                {
                    throw new InvalidDataException(string.Format("The second field is {0}. Expected: 42", fourtyTwo));
                }

                tiffHeader.ifdList = new List<ImageFileDirectory>();

                UInt32 ifdOffset;
                while ((ifdOffset = reader.ReadUInt32()) != 0)
                {
                    reader.Seek(ifdOffset);
                    tiffHeader.ifdList.Add(ImageFileDirectory.Deserialize(reader));
                }
            }

            return tiffHeader;
        }

        public ImageFileDirectory GetIFD(int index)
        {
            return ifdList[index];
        }

        public IfdTag FindTag(TagId tagId)
        {
            foreach (var ifd in ifdList)
            {
                var tag = ifd.FindTag(tagId);
                if (tag != null)
                {
                    return tag;
                }
            }

            return null;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ifd in this.ifdList)
            {
                sb.AppendLine("----------------------------------------");
                sb.Append(ifd.ToString());
            }

            sb.AppendLine("----------------------------------------");
            return sb.ToString();
        }

        private static bool FindBeginning(Stream stream)
        {
            long position = stream.Position;

            byte[] data = new byte[4];

            if (stream.Read(data, 0, 2) != 2)
            {
                throw Log.TraceException(new InvalidDataException());
            }

            if (data[0] == 0xFF && data[1] == 0xD8)
            {
                while (true)
                {
                    if (stream.Read(data, 0, 4) != 4)
                    {
                        throw Log.TraceException(new InvalidDataException());
                    }

                    if (data[0] != 0xFF)
                    {
                        throw Log.TraceException(new InvalidDataException());
                    }

                    if (data[1] == 0xE1)
                    {
                        break;
                    }

                    if (data[1] == 0xDA)
                    {
                        return false;
                    }

                    Log.TraceEvent(TraceEventType.Verbose, 0, "Skip tag: {0} {1} goto offset: {2}", data[0], data[1], data[2] * 256 + data[3] - 2);
                    stream.Seek(data[2] * 256 + data[3] - 2, SeekOrigin.Current);
                }
            
                if (stream.Read(data, 0, 4) != 4)
                {
                    throw Log.TraceException(new InvalidDataException());
                }

                if (!Encoding.ASCII.GetString(data).Equals("Exif"))
                {
                    throw Log.TraceException(new InvalidDataException());
                }

                if (stream.Read(data, 0, 2) != 2)
                {
                    throw Log.TraceException(new InvalidDataException());
                }

                if (data[0] != 0x0 || data[1] != 0x0)
                {
                    throw Log.TraceException(new InvalidDataException());
                }
            }
            else
            {
                stream.Seek(position, SeekOrigin.Begin);
            }

            return true;
        }
    }
}
