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
    public class DeleteItemCommand : IItemCommand
    {
        private readonly ItemChange _itemChange;

        public DeleteItemCommand(ItemChange itemChange)
        {
            _itemChange = itemChange;
        }

        public async Task ExecuteOnDestinationAsync(AmazonS3Client s3Client, string bucketName, CancellationToken token)
        {
            var response = await DeleteItemAsync(s3Client, bucketName, token);

            if (response.HttpStatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException(string.Format("Failed to delete {0} from S3", _itemChange.Item.Path));
            }
        }

        private async Task<DeleteObjectResponse> DeleteItemAsync(AmazonS3Client s3Client, string bucketName, CancellationToken token)
        {
            var request = new DeleteObjectRequest()
            {
                BucketName = bucketName,
                Key = _itemChange.Item.Path
            };
            return await s3Client.DeleteObjectAsync(request, token);
        }

        public IEnumerable<string> GetDescription(string format)
        {
            return new string[] { string.Format(format, _itemChange.Item.Path, Resources.DeleteItemCommand) };
        }
    }
}
