using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDownloadRequest
    {
        Task<IEnumerable<ItemChange>> DownloadChangesInCommitAsync(string commitId, Guid repositoryId, CancellationToken token);

        Task DownloadItemAndContentInCommitAsync(ItemChange change, string commitId, Guid repositoryId, CancellationToken token);
    }
}
