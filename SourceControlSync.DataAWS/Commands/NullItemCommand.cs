using SourceControlSync.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class NullItemCommand : IItemCommand
    {
        public string ConnectionString { get; set; }

        public Task ExecuteOnS3BucketAsync(ItemChange itemChange, CancellationToken token)
        {
            return Task.FromResult(0);
        }
    }
}
