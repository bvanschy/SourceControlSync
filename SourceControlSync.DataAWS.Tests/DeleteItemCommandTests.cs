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
        [TestMethod]
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

        private static IItemCommand CreateDeleteCommand()
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

            return new DeleteItemCommand(connectionStringBuilder.ConnectionString);
        }
    }
}
