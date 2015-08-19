using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.DataAWS;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class AWSS3RepositoryTests
    {
        [TestMethod]
        public void DeleteBlob()
        {
            var itemChanges = new List<ItemChange>();
            var itemChangeDeleteFile = new ItemChange()
            {
                ChangeType = ItemChangeType.Delete,
                Item = new Item()
                {
                    Path = "/test/test.txt"
                }
            };
            itemChanges.Add(itemChangeDeleteFile);
            var repo = CreateAWSS3Repository();

            repo.PushItemChangesAsync(itemChanges, "/").Wait();
        }

        [TestMethod]
        public void UploadBlob()
        {
            var itemChanges = new List<ItemChange>();
            var itemChangeUpload = new ItemChange()
            {
                ChangeType = ItemChangeType.Add,
                Item = new Item()
                {
                    ContentMetadata = new FileContentMetadata()
                    {
                        ContentType = "text/plain",
                        Encoding = Encoding.UTF8
                    },
                    Path = "/test/test.txt"
                },
                NewContent = new ItemContent()
                {
                    ContentType = ItemContentType.RawText,
                    Content = "This is a test"
                }
            };
            itemChanges.Add(itemChangeUpload);
            var repo = CreateAWSS3Repository();

            repo.PushItemChangesAsync(itemChanges, "/").Wait();
        }

        private static AWSS3Repository CreateAWSS3Repository()
        {
            var repo = new AWSS3Repository(
                ConfigurationManager.AppSettings["AWS-AccessKeyId"],
                ConfigurationManager.AppSettings["AWS-SecretAccessKey"],
                ConfigurationManager.AppSettings["AWS-SystemName"],
                ConfigurationManager.AppSettings["AWS-BucketName"]);
            return repo;
        }
    }
}
