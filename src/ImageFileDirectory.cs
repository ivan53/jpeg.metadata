using Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMetadata
{
    public class ImageFileDirectory
    {
        private static LogSource Log = new LogSource("ImageFileDirectory", LogSource.SourceLevel("ImageFileDirectory"));

        private List<IfdTag> ifdItems;

        public ImageFileDirectory()
        {
            this.ifdItems = new List<IfdTag>();
        }

        public IfdTag GetItem(int index)
        {
            return ifdItems[index];
        }

        public static ImageFileDirectory Deserialize(DataReader reader)
        {
            UInt16 items = reader.ReadUInt16();
            var ifd = new ImageFileDirectory();
            ifd.ifdItems = new List<IfdTag>(items);
            Log.TraceEvent(TraceEventType.Verbose, 0, "({0}) Deserialize with {0} items.", items);

            for (int i = 0; i < items; i++)
            {
                ifd.ifdItems.Add(IfdTag.Deserialize(reader));
            }

            return ifd;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (IfdTag item in ifdItems)
            {
                sb.AppendFormat(string.Format("{0}({1}) = {2}", item.TagId, item.TypeId, item.ToString()));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public IfdTag FindTag(TagId tagId)
        {
            foreach (var tag in ifdItems)
            {
                if (tag.TagId == tagId)
                {
                    return tag;
                }

                if (tag.TypeId == IfdTagType.PrivateIFD)
                {
                    for (int i = 0; i < tag.Count; i++)
                    {
                        var ifd = (ImageFileDirectory)tag.GetData(i);
                        var findTag = ifd.FindTag(tagId);
                        if (findTag != null)
                        {
                            return findTag;
                        }
                    }
                }
            }

            return null;
        }
    }
}
