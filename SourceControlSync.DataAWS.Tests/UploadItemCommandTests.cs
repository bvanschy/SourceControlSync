using Amazon.S3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Configuration;
using System.Text;
using System.Threading;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class UploadItemCommandTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
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
    }
}
