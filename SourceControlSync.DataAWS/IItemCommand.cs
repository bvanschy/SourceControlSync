using Amazon.S3;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public interface IItemCommand
    {
        Task ExecuteOnDestinationAsync(AmazonS3Client s3Client, string bucketName, CancellationToken token);

        IEnumerable<string> GetDescription(string format);
    }
}
