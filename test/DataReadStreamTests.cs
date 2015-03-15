using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using ImageMetadata;

namespace ImageMetadataUT
{
    [TestClass]
    public class DataReadStreamTests : TestBase
    {
        [TestMethod]
        public void ReadTest()
        {
            string test = "Test data";

            using (var bufferedStream = new BufferedDataReadStream(GetStream(test)))
            {
                byte[] data = new byte[1];
                bufferedStream.Read(data, 0, data.Length);
                Assert.AreEqual("T", Encoding.ASCII.GetString(data));

                Assert.AreEqual(1, bufferedStream.Position);

                bufferedStream.Seek(5, SeekOrigin.Begin);

                data = new byte[4];
                bufferedStream.Read(data, 0, data.Length);
                Assert.AreEqual("data", Encoding.ASCII.GetString(data));

                bufferedStream.Seek(-5, SeekOrigin.Current);
                data = new byte[1];
                bufferedStream.Read(data, 0, data.Length);
                Assert.AreEqual(" ", Encoding.ASCII.GetString(data));
            }
        }

        [TestMethod]
        public void SeekTest()
        {
            string test = "Test data";

            using (var bufferedStream = new BufferedDataReadStream(GetStream(test)))
            {
                Assert.AreEqual(0, bufferedStream.Position);

                bufferedStream.Seek(4, SeekOrigin.Current);
                Assert.AreEqual(4, bufferedStream.Position);

                bufferedStream.Seek(-1, SeekOrigin.Current);
                Assert.AreEqual(3, bufferedStream.Position);

                bufferedStream.Seek(8, SeekOrigin.Begin);
                Assert.AreEqual(8, bufferedStream.Position);

                AssertException(() => bufferedStream.Seek(0, SeekOrigin.End), typeof(NotSupportedException));
            }
        }

        private Stream GetStream(string data)
        {
            return new MemoryStream(Encoding.ASCII.GetBytes(data));
        }
    }
}
