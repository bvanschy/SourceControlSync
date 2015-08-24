using SourceControlSync.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public interface IItemCommand
    {
        string ConnectionString { set; }

        Task ExecuteOnS3BucketAsync(ItemChange itemChange, CancellationToken token);
    }
}
