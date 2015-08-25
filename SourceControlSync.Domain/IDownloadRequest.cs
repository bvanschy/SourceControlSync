using SourceControlSync.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDownloadRequest : IDisposable
    {
        Task DownloadChangesInCommitAsync(Commit commit, Guid repositoryId, CancellationToken token);
        Task DownloadItemAndContentInCommitAsync(ItemChange change, Commit commit, Guid repositoryId, CancellationToken token);
    }
}
