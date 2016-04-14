using SourceControlSync.Domain.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    /// <summary>
    /// Generic source repository client requiring a specific source control downloader
    /// </summary>
    public class SourceRepository : ISourceRepository
    {
        private readonly IDownloadRequest _downloadRequest;

        private Push _push;
        private string _root;
        private CancellationToken _token;

        public SourceRepository(IDownloadRequest downloadRequest)
        {
            _downloadRequest = downloadRequest;
        }

        public async Task DownloadChangesAsync(Push push, string root, CancellationToken token)
        {
            _push = push;
            _root = root;
            _token = token;

            await DownloadChangesInAllCommitsAsync();
        }

        private async Task DownloadChangesInAllCommitsAsync()
        {
            var itemTasks = from commit in _push.Commits
                            select DownloadCommitAsync(commit);
            await Task.WhenAll(itemTasks);
        }

        private async Task DownloadCommitAsync(Commit commit)
        {
            await DownloadChangesInCommitAsync(commit);
            await DownloadItemAndContentInChangesAsync(commit);
        }

        private async Task DownloadChangesInCommitAsync(Commit commit)
        {
            var changes = await _downloadRequest.DownloadChangesInCommitAsync(commit.CommitId, _push.Repository.Id, _token);
            commit.Changes = changes.Where(c => c.Item.IsInRoot(_root));
        }

        private async Task DownloadItemAndContentInChangesAsync(Commit commit)
        {
            var tasks = from change in commit.Changes
                        where ChangeTypeHasContent(change)
                        select _downloadRequest.DownloadItemAndContentInCommitAsync(change, commit.CommitId, _push.Repository.Id, _token);
            await Task.WhenAll(tasks);
        }

        private static bool ChangeTypeHasContent(ItemChange change)
        {
            return (change.ChangeType & (ItemChangeType.Edit | ItemChangeType.Add | ItemChangeType.Rename)) != 0;
        }
    }
}
