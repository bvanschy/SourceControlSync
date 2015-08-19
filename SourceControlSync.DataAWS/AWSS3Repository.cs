using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class AWSS3Repository : IDestinationRepository
    {
        private string _accessKeyId;
        private string _secretAccessKey;
        private RegionEndpoint _region;
        private string _bucketName;

        public AWSS3Repository(string accessKeyId, string secretAccessKey, string systemName, string bucketName)
        {
            _accessKeyId = accessKeyId;
            _secretAccessKey = secretAccessKey;
            _region = RegionEndpoint.GetBySystemName(systemName);
            _bucketName = bucketName;
        }

        public async Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root)
        {
            if (root == null)
            {
                throw new ArgumentNullException("root");
            }

            using (var s3Client = new AmazonS3Client(_accessKeyId, _secretAccessKey, _region))
            {
                var tasks = new List<Task>();
                foreach (var itemChange in changes.Where(c => !string.IsNullOrEmpty(c.Item.Path) && c.Item.Path.StartsWith(root)))
                {
                    if ((itemChange.ChangeType & ItemChangeType.Delete) != 0)
                    {
                        tasks.Add(DeleteItemAsync(s3Client, itemChange, root));
                    }
                    else if (itemChange.NewContent != null)
                    {
                        tasks.Add(UploadItemAsync(s3Client, itemChange, root));
                    }
                }
                await Task.WhenAll(tasks);
            }
        }

        private async Task DeleteItemAsync(AmazonS3Client s3Client, ItemChange itemChange, string root)
        {
            var request = new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = itemChange.Item.Path.Substring(root.Length)
            };
            var response = await s3Client.DeleteObjectAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException(string.Format("Failed to delete {0} from S3", itemChange.Item.Path));
            }
        }

        private async Task UploadItemAsync(AmazonS3Client s3Client, ItemChange itemChange, string root)
        {
            using (var contentStream = itemChange.CreateContentStream())
            {
                var request = new PutObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = itemChange.Item.Path.Substring(root.Length),
                    ContentType = itemChange.Item.ContentMetadata.ContentType,
                    InputStream = contentStream
                };
                var response = await s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException(string.Format("Failed to upload {0} to S3", itemChange.Item.Path));
                }
            }
        }
    }
}
