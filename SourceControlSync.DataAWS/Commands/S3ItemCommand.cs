using Amazon.S3;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public abstract class S3ItemCommand : IItemCommand
    {
        protected Bucket _bucket { get; set; }
        private Credentials _credentials { get; set; }

        public string ConnectionString
        {
            set
            {
                var connectionStringBuilder = new AWSS3ConnectionStringBuilder(value);
                _bucket = connectionStringBuilder.Bucket;
                _credentials = connectionStringBuilder.Credentials;
            }
        }

        protected void ValidateConnectionParameters()
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

        protected AmazonS3Client CreateS3Client()
        {
            return new AmazonS3Client(_credentials.AccessKeyId, _credentials.SecretAccessKey, _bucket.Region);
        }


        public abstract Task ExecuteOnS3BucketAsync(ItemChange itemChange, CancellationToken token);
    }
}
