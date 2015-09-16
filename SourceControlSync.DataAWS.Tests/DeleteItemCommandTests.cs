using Amazon;
using Amazon.S3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class DeleteItemCommandTests
    {
        private CultureInfo _culture;
        private CultureInfo _uiCulture;

        public TestContext TestContext { get; set; }

        [TestMethod]
        [Ignore]
        public void DeleteBlob()
        {
            var itemChange = CreateItemChange();
            var command = CreateDeleteCommand(itemChange);
            var s3Client = CreateS3Client();

            command.ExecuteOnDestinationAsync(s3Client, GetBucketName(), CancellationToken.None).Wait();
        }

        [TestMethod]
        public void CommandToString()
        {
            var itemChange = CreateItemChange();
            var command = CreateDeleteCommand(itemChange);

            var description = command.GetDescription("{0} {1}");

            Assert.IsTrue(description.Single().Contains("test/test.txt"));
            Assert.IsTrue(description.Single().Contains("Deleted"));
        }

        [TestMethod]
        public void CommandToStringInFrench()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
            var itemChange = CreateItemChange();
            var command = CreateDeleteCommand(itemChange);

            var description = command.GetDescription("{0} {1}");

            Assert.IsTrue(description.Single().Contains("test/test.txt"));
            Assert.IsTrue(description.Single().Contains("supprim"));
        }

        private ItemChange CreateItemChange()
        {
            return new ItemChange(ItemChangeType.Delete, new Item("test/test.txt"));
        }

        private IItemCommand CreateDeleteCommand(ItemChange itemChange)
        {
            return new DeleteItemCommand(itemChange);
        }

        private AmazonS3Client CreateS3Client()
        {
            var region = RegionEndpoint.GetBySystemName(TestContext.Properties["AWSRegionSystemName"] as string);
            return new AmazonS3Client(
                TestContext.Properties["AWSAccessKeyId"] as string, 
                TestContext.Properties["AWSSecretAccessKey"] as string, 
                region
                );
        }

        private string GetBucketName()
        {
            return TestContext.Properties["AWSBucketName"] as string;
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
