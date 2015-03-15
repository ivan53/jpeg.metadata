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
    public class IfdTag
    {
        private static LogSource Log = new LogSource("IfdTag", LogSource.SourceLevel("IfdTag"));

        object[] data;

        private static List<TagId> privateIFD = new List<TagId> { TagId.ExifIFD, TagId.GpsIFD, TagId.InteropIFD };

        public TagId TagId { get; private set; }

        public IfdTagType TypeId { get; private set; }

        public UInt32 Count { get; private set; }

        public static IfdTag Deserialize(DataReader reader)
        {
            var item = new IfdTag()
            {
                TagId = (TagId)reader.ReadUInt16(),
                TypeId = (IfdTagType)reader.ReadUInt16(),
                Count = reader.ReadUInt32(),
            };

            if (privateIFD.Contains(item.TagId))
            {
                item.TypeId = IfdTagType.PrivateIFD;
            }

            long position = reader.Position;
            if (item.Count < 4096)
            {
                item.data = new object[item.Count];

                switch (item.TypeId)
                {
                    case IfdTagType.AscII:
                        ReadAscII(reader, item);
                        break;
                    case IfdTagType.Byte:
                    case IfdTagType.Undefined:
                        ReadByte(reader, item);
                        break;
                    case IfdTagType.Short:
                        ReadShort(reader, item);
                        break;
                    case IfdTagType.Long:
                        ReadLong(reader, item);
                        break;
                    case IfdTagType.Rational:
                        ReadRational(reader, item);
                        break;
                    case IfdTagType.SignedLong:
                        ReadSignedLong(reader, item);
                        break;
                    case IfdTagType.SignedRational:
                        ReadSignedRational(reader, item);
                        break;
                    case IfdTagType.PrivateIFD:
                        ReadIfd(reader, item);
                        break;
                    default:
                        throw Log.TraceException(new NotImplementedException(item.TypeId.ToString()));
                }
            }
            else
            {
                if (item.TagId != TagId.MakerNote)
                {
                    Log.TraceEvent(TraceEventType.Warning, 0, "Too long. Type: {0}, Data: {1}, Count: {2}", item.TagId, item.TypeId, item.Count);
                }
            }

            reader.Seek(position + 4); // set the position after the value
            return item;
        }

        public object GetData(int index)
        {
            return data[index];
        }

        public override string ToString()
        {
            var result = string.Empty;

            if (this.Count > 1)
            {
                result += "[";
            }
            if (this.TypeId == IfdTagType.PrivateIFD)
            {
                result += Environment.NewLine;
            }
            if (this.data != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (i > 0 && this.TypeId != IfdTagType.AscII)
                    {
                        if (this.TypeId == IfdTagType.PrivateIFD)
                        {
                            result += Environment.NewLine;
                        }
                        else
                        {
                            result += ",";
                        }
                    }

                    RationalType rational;

                    switch (this.TypeId)
                    {
                        case IfdTagType.AscII:
                            if ((Char)data[i] != '\0')
                            {
                                result += (Char)data[i];
                            }
                            break;
                        case IfdTagType.Byte:
                        case IfdTagType.Undefined:
                            result += ((Byte)data[i]).ToString();
                            break;
                        case IfdTagType.Short:
                            result += ((UInt16)data[i]).ToString();
                            break;
                        case IfdTagType.Long:
                            result += ((UInt32)data[i]).ToString();
                            break;
                        case IfdTagType.Rational:
                        case IfdTagType.SignedRational:
                            rational = (RationalType)data[i];
                            result += rational.Numerator.ToString();
                            if (rational.Denomerator != 1 && rational.Numerator != 0)
                            {
                                result += "/" + rational.Denomerator.ToString();
                            }

                            break;
                        case IfdTagType.SignedLong:
                            result += ((Int32)data[i]).ToString();
                            break;
                        case IfdTagType.PrivateIFD:
                            result += ((ImageFileDirectory)data[i]).ToString();
                            break;
                        default:
                            throw Log.TraceException(new NotImplementedException(this.TypeId.ToString()));
                    }
                }
            }
            else
            {
                result += "*** Not loaded ***";
            }
            if (this.Count > 1)
            {
                result += "]";
            }

            return result;
        }

        private static void ReadIfd(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt32)))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    UInt32 offset = dataReader.ReadUInt32();
                    reader.Seek(offset);
                    item.data[i] = ImageFileDirectory.Deserialize(reader);
                }
            }
        }

        private static void ReadSignedRational(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt32) * 2))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    RationalType data = new RationalType
                    {
                        Numerator = dataReader.ReadInt32(),
                        Denomerator = dataReader.ReadUInt32(),
                    };

                    item.data[i] = data;
                }
            }
        }

        private static void ReadSignedLong(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt32)))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item.data[i] = dataReader.ReadInt32();
                }
            }
        }

        private static void ReadRational(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt32) * 2))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    RationalType data = new RationalType
                    {
                        Numerator = dataReader.ReadUInt32(),
                        Denomerator = dataReader.ReadUInt32(),
                    };
                    item.data[i] = data;
                }
            }
        }

        private static void ReadShort(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt16)))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item.data[i] = dataReader.ReadUInt16();
                }
            }
        }

        private static void ReadLong(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(UInt32)))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item.data[i] = dataReader.ReadUInt32();
                }
            }
        }

        private static void ReadByte(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(Byte)))
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item.data[i] = dataReader.ReadByte();
                }
            }
        }

        private static void ReadAscII(DataReader reader, IfdTag item)
        {
            using (var dataReader = GetDataReader(reader, item.Count * sizeof(Byte)))
            {
                char[] chars = Encoding.ASCII.GetChars(dataReader.ReadBytes((int)item.Count));

                for (int i = 0; i < item.Count; i++)
                {
                    item.data[i] = chars[i];
                }
            }
        }

        private static DataReader GetDataReader(DataReader reader, UInt32 countBytes)
        {
            byte[] data = new byte[countBytes];
            if (countBytes > 4)
            {
                UInt32 offset = reader.ReadUInt32();
                reader.Seek(offset);
            }

            data = reader.ReadBytes(data.Length);

            var dataReader = new DataReader(new MemoryStream(data));
            dataReader.LittleEndian = reader.LittleEndian;
            return dataReader;
        }

    }
}
