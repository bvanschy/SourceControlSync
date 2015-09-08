using Amazon;
using Amazon.S3;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class AWSS3Context : IDestinationContext
    {
        private readonly Bucket _bucket;
        private readonly Credentials _credentials;
        private readonly List<ItemChange> _itemChanges = new List<ItemChange>();
        private readonly IList<IItemCommand> _executedCommands = new List<IItemCommand>();

        public AWSS3Context(string connectionString)
        {
            var connectionStringBuilder = new AWSS3ConnectionStringBuilder(connectionString);
            _bucket = connectionStringBuilder.Bucket;
            _credentials = connectionStringBuilder.Credentials;
        }

        public IExecutedCommands ExecutedCommands
        {
            get { return new ExecutedCommands(_executedCommands); }
        }

        public void AddItemChanges(IEnumerable<ItemChange> itemChanges)
        {
            _itemChanges.AddRange(itemChanges);
        }

        public async Task SaveChangesAsync(CancellationToken token)
        {
            _executedCommands.Clear();

            var commands = _itemChanges.CreateItemCommands();
            using (var s3Client = CreateS3Client())
            {
                foreach (var command in commands)
                {
                    await command.ExecuteOnDestinationAsync(s3Client, _bucket.BucketName, token);
                    _executedCommands.Add(command);
                }
            }

            _itemChanges.Clear();
        }

        private AmazonS3Client CreateS3Client()
        {
            ValidateConnectionParameters();
            var region = RegionEndpoint.GetBySystemName(_bucket.RegionSystemName);
            return new AmazonS3Client(_credentials.AccessKeyId, _credentials.SecretAccessKey, region);
        }

        private void ValidateConnectionParameters()
        {
            if (_bucket == null ||
                string.IsNullOrWhiteSpace(_bucket.BucketName) ||
                string.IsNullOrWhiteSpace(_bucket.RegionSystemName))
            {
                throw new ApplicationException("Invalid bucket");
            }
            if (_credentials == null ||
                string.IsNullOrWhiteSpace(_credentials.AccessKeyId) ||
                string.IsNullOrWhiteSpace(_credentials.SecretAccessKey))
            {
                throw new ApplicationException("Invalid credentials");
            }
        }
    }
}
