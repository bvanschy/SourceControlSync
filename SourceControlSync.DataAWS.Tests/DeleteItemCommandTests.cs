using Amazon.S3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class DeleteItemCommandTests
    {
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
    }
}
