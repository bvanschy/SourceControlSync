using Amazon.S3;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class NullItemCommand : IItemCommand
    {
        public Task ExecuteOnDestinationAsync(AmazonS3Client s3Client, string bucketName, string path, CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public IEnumerable<string> GetDescription(string format)
        {
            return Enumerable.Empty<string>();
        }
    }
}
