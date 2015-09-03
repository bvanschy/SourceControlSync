using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System.Globalization;
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
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Delete,
                Item = new Item()
                {
                    Path = "test/test.txt"
                }
            };
            var command = CreateDeleteCommand();

            command.ExecuteOnDestinationAsync(itemChange, CancellationToken.None).Wait();

            Assert.IsTrue(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void DeleteBlobAllowed()
        {
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Delete | ItemChangeType.SourceRename
            };
            var command = CreateDeleteCommand();

            Assert.IsTrue(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void AddBlobNotAllowed()
        {
            var itemChange = new ItemChange()
            {
                ChangeType = ItemChangeType.Add
            };
            var command = CreateDeleteCommand();

            Assert.IsFalse(command.IsChangeOperable(itemChange));
        }

        [TestMethod]
        public void CommandToString()
        {
            var command = CreateDeleteCommand();

            var description = command.ToString();

            Assert.AreEqual("Deleted", description);
        }

        [TestMethod]
        public void CommandToStringInFrench()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
            var command = CreateDeleteCommand();

            var description = command.ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(description));
            Assert.AreNotEqual("Deleted", description);
        }

        private IItemCommand CreateDeleteCommand()
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
            return new DeleteItemCommand(bucket, credentials);
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
