using Amazon.S3.Model;
using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class UploadItemCommand : S3ItemCommand
    {
        public UploadItemCommand(string connectionString)
            : base(connectionString)
        {
        }

        public override async Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token)
        {
            var response = await UploadItemAsync(itemChange, token);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException(string.Format("Failed to upload {0} to S3", itemChange.Item.Path));
            }
        }

        private async Task<PutObjectResponse> UploadItemAsync(ItemChange itemChange, CancellationToken token)
        {
            using (var contentStream = itemChange.CreateContentStream())
            {
                var request = new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = itemChange.Item.Path,
                    ContentType = itemChange.Item.ContentMetadata.ContentType,
                    InputStream = contentStream
                };
                var s3Client = CreateS3Client();
                return await s3Client.PutObjectAsync(request, token);
            }
        }
    }
}
