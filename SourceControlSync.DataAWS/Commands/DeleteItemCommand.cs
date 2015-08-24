using Amazon.S3;
using Amazon.S3.Model;
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
    public class DeleteItemCommand : S3ItemCommand
    {
        public override async Task ExecuteOnS3BucketAsync(ItemChange itemChange, CancellationToken token)
        {
            ValidateConnectionParameters();

            var response = await DeleteItemAsync(itemChange, token);

            if (response.HttpStatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException(string.Format("Failed to delete {0} from S3", itemChange.Item.Path));
            }
        }

        private async Task<DeleteObjectResponse> DeleteItemAsync(ItemChange itemChange, CancellationToken token)
        {
            using (var s3Client = CreateS3Client())
            {
                var request = new DeleteObjectRequest()
                {
                    BucketName = _bucket.BucketName,
                    Key = itemChange.Item.Path
                };
                return await s3Client.DeleteObjectAsync(request, token);
            }
        }
    }
}
