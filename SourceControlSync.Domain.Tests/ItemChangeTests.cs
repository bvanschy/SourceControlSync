using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Models;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ItemChangeTests
    {
        [TestMethod]
        public void CreateTextStream()
        {
            var item = new Item(string.Empty)
            {
                ContentMetadata = new FileContentMetadata("text/plain", Encoding.UTF8)
            };
            var itemChange = new ItemChange(ItemChangeType.None, item)
            {
                NewContent = new ItemContent(ItemContentType.RawText, "Testing")
            };

            using (var stream = itemChange.CreateContentStream())
            using (var streamReader = new StreamReader(stream, itemChange.Item.ContentMetadata.Encoding))
            {
                Assert.AreEqual("Testing", streamReader.ReadToEnd());
            }
        }

        [TestMethod]
        public void CreateBinaryStream()
        {
            var item = new Item(string.Empty)
            {
                ContentMetadata = new FileContentMetadata("image/x-icon")
            };
            byte[] content = Encoding.UTF8.GetBytes("Testing");
            var itemChange = new ItemChange(ItemChangeType.None, item)
            {
                NewContent = new ItemContent(ItemContentType.Base64Encoded, Convert.ToBase64String(content))
            };

            using (var stream = itemChange.CreateContentStream())
            {
                byte[] streamBytes = new byte[16];
                Assert.AreEqual(7, stream.Read(streamBytes, 0, 16));
                Assert.IsTrue(content.SequenceEqual(streamBytes.Take(7)));
            }
        }

        [TestMethod]
        public void CreateTextItemContent()
        {
            var item = new Item(string.Empty)
            {
                ContentMetadata = new FileContentMetadata("text/plain", Encoding.UTF8)
            };
            var itemChange = new ItemChange(ItemChangeType.None, item);

            var testData = "Testing";
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testData)))
            {
                itemChange.SetNewContentAsync(memoryStream, CancellationToken.None).Wait();

                Assert.AreEqual(ItemContentType.RawText, itemChange.NewContent.ContentType);
                Assert.AreEqual(testData, itemChange.NewContent.Content);
            }
        }

        [TestMethod]
        public void CreateBinaryItemContext()
        {
            var item = new Item(string.Empty)
            {
                ContentMetadata = new FileContentMetadata("image/x-icon")
            };
            var itemChange = new ItemChange(ItemChangeType.None, item);

            var testData = Encoding.UTF8.GetBytes("Testing");
            using (var memoryStream = new MemoryStream(testData))
            {
                itemChange.SetNewContentAsync(memoryStream, CancellationToken.None).Wait();

                Assert.AreEqual(ItemContentType.Base64Encoded, itemChange.NewContent.ContentType);
                Assert.AreEqual(Convert.ToBase64String(testData), itemChange.NewContent.Content);
            }
        }
    }
}
