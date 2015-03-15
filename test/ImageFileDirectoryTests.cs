using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageMetadata;
using System.Diagnostics;
using Logger;

namespace ImageMetadataUT
{
    [TestClass]
    public class ImageFileDirectoryTests : TestBase
    {
        private static LogSource Log = new LogSource("ImageFileDirectoryTests", LogSource.SourceLevel("ImageFileDirectoryTests"));

        [TestMethod]
        public void SimpleCheck()
        {
            using (var stream = File.Open(@"Test1.JPG", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(12, SeekOrigin.Begin);
                using (var reader = new DataReader(new BufferedDataReadStream(stream)))
                {
                    reader.LittleEndian = true;
                    reader.Seek(8);
                    var ifd = ImageFileDirectory.Deserialize(reader);
                    Log.TraceEvent(TraceEventType.Information, 0, ifd.ToString());
                }
            }
        }

        [TestMethod]
        public void GetTagTest()
        {
            ImageFileDirectory ifd;
            const string expected = "2001:07:07 11:49:50\0";

            using (var stream = File.Open(@"Test1.JPG", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                stream.Seek(12, SeekOrigin.Begin);
                using (var reader = new DataReader(new BufferedDataReadStream(stream)))
                {
                    reader.LittleEndian = true;
                    reader.Seek(8);
                    ifd = ImageFileDirectory.Deserialize(reader);
                }
            }

            IfdTag tag = ifd.FindTag(TagId.DateTimeOriginal);
            Assert.AreEqual(IfdTagType.AscII, tag.TypeId);
            Assert.AreEqual(expected.Length, (int)tag.Count);
            for (int i = 0; i < tag.Count; i++)
            {
                Assert.AreEqual(expected[i], tag.GetData(i));
            }
        }
    }
}
