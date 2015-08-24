using Amazon.S3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            command.ExecuteOnS3BucketAsync(itemChange, CancellationToken.None).Wait();
        }

        private static IItemCommand CreateUploadCommand()
        {
            var connectionStringBuilder = new AWSS3ConnectionStringBuilder()
            {
                Bucket = new Bucket()
                {
                    RegionSystemName = ConfigurationManager.AppSettings["AWS-SystemName"],
                    BucketName = ConfigurationManager.AppSettings["AWS-BucketName"]
                },
                Credentials = new Credentials()
                {
                    AccessKeyId = ConfigurationManager.AppSettings["AWS-AccessKeyId"],
                    SecretAccessKey = ConfigurationManager.AppSettings["AWS-SecretAccessKey"]
                }
            };

            return new UploadItemCommand()
            {
                ConnectionString = connectionStringBuilder.ConnectionString
            };
        }
    }
}
