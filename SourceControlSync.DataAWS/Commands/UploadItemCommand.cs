using Amazon.S3;
using Amazon.S3.Model;
using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class UploadItemCommand : S3ItemCommand
    {
        public override async Task ExecuteOnS3BucketAsync(ItemChange itemChange, CancellationToken token)
        {
            ValidateConnectionParameters();

            var response = await UploadItemAsync(itemChange, token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException(string.Format("Failed to upload {0} to S3", itemChange.Item.Path));
            }
        }

        private async Task<PutObjectResponse> UploadItemAsync(ItemChange itemChange, CancellationToken token)
        {
            using (var contentStream = itemChange.CreateContentStream())
            using (var s3Client = CreateS3Client())
            {
                var request = new PutObjectRequest()
                {
                    BucketName = _bucket.BucketName,
                    Key = itemChange.Item.Path,
                    ContentType = itemChange.Item.ContentMetadata.ContentType,
                    InputStream = contentStream
                };
                return await s3Client.PutObjectAsync(request, token);
            }
        }
    }
}
