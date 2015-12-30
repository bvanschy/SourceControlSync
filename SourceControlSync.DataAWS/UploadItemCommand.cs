using Amazon.S3;
using Amazon.S3.Model;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class UploadItemCommand : IItemCommand
    {
        private readonly ItemChange _itemChange;

        public UploadItemCommand(ItemChange itemChange)
        {
            _itemChange = itemChange;
        }

        public async Task ExecuteOnDestinationAsync(AmazonS3Client s3Client, string bucketName, string path, CancellationToken token)
        {
            var response = await UploadItemAsync(s3Client, bucketName, path, token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException(string.Format("Failed to upload {0} to S3", _itemChange.Item.Path));
            }
        }

        private async Task<PutObjectResponse> UploadItemAsync(AmazonS3Client s3Client, string bucketName, string path, CancellationToken token)
        {
            using (var contentStream = _itemChange.CreateContentStream())
            {
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = path + _itemChange.Item.Path,
                    ContentType = _itemChange.Item.ContentMetadata.ContentType,
                    InputStream = contentStream
                };
                return await s3Client.PutObjectAsync(request, token);
            }
        }

        public IEnumerable<string> GetDescription(string format)
        {
            return new string[] { string.Format(format, _itemChange.Item.Path, Resources.UploadItemCommand) };
        }
    }
}
