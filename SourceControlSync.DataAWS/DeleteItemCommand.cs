using Amazon.S3.Model;
using SourceControlSync.Domain.Models;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class DeleteItemCommand : S3ItemCommand
    {
        public DeleteItemCommand(string connectionString)
            : base(connectionString)
        {
        }

        public DeleteItemCommand(Bucket bucket, Credentials credentials)
            : base(bucket, credentials)
        {
        }

        public override bool IsChangeOperable(ItemChange itemChange)
        {
            return (itemChange.ChangeType & ItemChangeType.Delete) != 0;
        }

        public override async Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token)
        {
            var response = await DeleteItemAsync(itemChange, token);

            if (response.HttpStatusCode != HttpStatusCode.NoContent)
            {
                throw new ApplicationException(string.Format("Failed to delete {0} from S3", itemChange.Item.Path));
            }
        }

        private async Task<DeleteObjectResponse> DeleteItemAsync(ItemChange itemChange, CancellationToken token)
        {
            var request = new DeleteObjectRequest()
            {
                BucketName = BucketName,
                Key = itemChange.Item.Path
            };
            var s3Client = CreateS3Client();
            return await s3Client.DeleteObjectAsync(request, token);
        }

        public override string ToString()
        {
            return Resources.DeleteItemCommand;
        }
    }
}
