using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System.Linq;
using System.Text;
using System.IO;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ItemChangeExtensionsTests
    {
        [TestMethod]
        public void CreateTextStream()
        {
            var itemChange = new ItemChange()
            {
                Item = new Item()
                {
                    ContentMetadata = new FileContentMetadata()
                    {
                        Encoding = Encoding.UTF8
                    }
                },
                NewContent = new ItemContent()
                {
                    ContentType = ItemContentType.RawText,
                    Content = "Testing"
                }
            };

            var stream = itemChange.CreateContentStream();

            using (var streamReader = new StreamReader(stream, itemChange.Item.ContentMetadata.Encoding))
            {
                Assert.AreEqual("Testing", streamReader.ReadToEnd());
            }
        }

        [TestMethod]
        public void CreateBinaryStream()
        {
            byte[] content = Encoding.UTF8.GetBytes("Testing");
            var itemChange = new ItemChange()
            {
                NewContent = new ItemContent()
                {
                    ContentType = ItemContentType.Base64Encoded,
                    Content = Convert.ToBase64String(content)
                }
            };

            var stream = itemChange.CreateContentStream();

            byte[] streamBytes = new byte[16];
            Assert.AreEqual(7, stream.Read(streamBytes, 0, 16));
            Assert.IsTrue(content.SequenceEqual(streamBytes.Take(7)));
        }
    }
}
