using Amazon.S3;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public abstract class S3ItemCommand : IItemCommand
    {
        protected readonly Bucket _bucket;
        protected readonly Credentials _credentials;

        protected AmazonS3Client _s3Client;

        public S3ItemCommand(string connectionString)
        {
            var connectionStringBuilder = new AWSS3ConnectionStringBuilder(connectionString);
            _bucket = connectionStringBuilder.Bucket;
            _credentials = connectionStringBuilder.Credentials;
        }

        protected void CreateS3Client()
        {
            if (_s3Client == null)
            {
                ValidateConnectionParameters();
                _s3Client = new AmazonS3Client(_credentials.AccessKeyId, _credentials.SecretAccessKey, _bucket.Region);
            }
        }

        private void ValidateConnectionParameters()
        {
            if (_bucket == null ||
                string.IsNullOrWhiteSpace(_bucket.BucketName) ||
                string.IsNullOrWhiteSpace(_bucket.RegionSystemName))
            {
                throw new ApplicationException("Invalid ConnectionString");
            }
            if (_credentials == null ||
                string.IsNullOrWhiteSpace(_credentials.AccessKeyId) ||
                string.IsNullOrWhiteSpace(_credentials.SecretAccessKey))
            {
                throw new ApplicationException("Invalid ConnectionString");
            }
        }

        public abstract Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token);

        public void Dispose()
        {
            if (_s3Client != null)
            {
                _s3Client.Dispose();
                _s3Client = null;
            }
        }
    }
}
