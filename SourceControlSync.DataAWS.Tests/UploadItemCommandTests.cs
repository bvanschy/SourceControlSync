using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System.Globalization;
using System.Text;
using System.Threading;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class UploadItemCommandTests
    {
        private CultureInfo _culture;
        private CultureInfo _uiCulture;

        public TestContext TestContext { get; set; }

        [TestMethod]
        [Ignore]
        public void UploadBlob()
        {
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Add,
                Item = new Item()
                {
                    ContentMetadata = new FileContentMetadata()
                    {
                        ContentType = "text/plain",
                        Encoding = Encoding.UTF8
                    },
                    Path = "test/test.txt"
                },
                NewContent = new ItemContent()
                {
                    ContentType = ItemContentType.RawText,
                    Content = "This is a test"
                }
            };
            var command = CreateUploadCommand();

            command.ExecuteOnDestinationAsync(itemChange, CancellationToken.None).Wait();

            Assert.IsTrue(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void UploadBlobAllowed()
        {
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Rename,
                NewContent = new ItemContent()
            };
            var command = CreateUploadCommand();

            Assert.IsTrue(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void UploadBlobNotAllowed()
        {
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Delete
            };
            var command = CreateUploadCommand();

            Assert.IsFalse(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void CommandToString()
        {
            var command = CreateUploadCommand();

            var description = command.ToString();

            Assert.AreEqual("Uploaded", description);
        }

        [TestMethod]
        public void CommandToStringInFrench()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
            var command = CreateUploadCommand();

            var description = command.ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(description));
            Assert.AreNotEqual("Uploaded", description);
        }

        private IItemCommand CreateUploadCommand()
        {
            var bucket = new Bucket()
            {
                RegionSystemName = TestContext.Properties["AWSRegionSystemName"] as string,
                BucketName = TestContext.Properties["AWSBucketName"] as string
            };
            var credentials = new Credentials()
            {
                AccessKeyId = TestContext.Properties["AWSAccessKeyId"] as string,
                SecretAccessKey = TestContext.Properties["AWSSecretAccessKey"] as string
            };
            return new UploadItemCommand(bucket, credentials);
        }

        [TestInitialize]
        public void InitializeTest()
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            _uiCulture = Thread.CurrentThread.CurrentUICulture;
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
        }
    }
}
