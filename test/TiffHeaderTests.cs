using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using ImageMetadata;
using System.Diagnostics;
using Logger;

namespace ImageMetadataUT
{
    [TestClass]
    public class TiffHeaderTests : TestBase
    {
        private static LogSource Log = new LogSource("ImageFileDirectoryTests", LogSource.SourceLevel("ImageFileDirectoryTests"));

        [TestMethod]
        public void SimpleCheck()
        {
            var tiffHeader = TiffHeader.Deserialize("Test1.JPG");
            Log.TraceEvent(TraceEventType.Information, 0, tiffHeader.ToString());
        }

        [TestMethod]
        public void GetTagTest()
        {
            var tiffHeader = TiffHeader.Deserialize("Test1.JPG");
            const string expected = "2001:07:07 11:49:50\0";

            IfdTag tag = tiffHeader.FindTag(TagId.DateTimeOriginal);
            Assert.AreEqual(TagId.DateTimeOriginal, tag.TagId);
            Assert.AreEqual(IfdTagType.AscII, tag.TypeId);
            Assert.AreEqual(expected.Length, (int)tag.Count);
            for (int i = 0; i < tag.Count; i++)
            {
                Assert.AreEqual(expected[i], tag.GetData(i));
            }
        }
    }
}
