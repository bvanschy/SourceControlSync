using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    class DownloadRequest
    {
        private readonly Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient _httpClient;

        private Push _push;
        private string _root;
        private CancellationToken _token;

        public DownloadRequest(Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task DownloadChangesInAllCommitsAsync(Push push, string root, CancellationToken token)
        {
            _push = push;
            _root = root;
            _token = token;

            var itemTasks = from commit in push.Commits
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
            var commitChanges = await _httpClient.GetChangesAsync(commit.CommitId, _push.Repository.Id,
                                        cancellationToken: _token);
            var changesInRoot = commitChanges.Changes.Where(c => c.Item.IsInRoot(_root));
            commit.Changes = changesInRoot.ToSync();
        }

        private async Task DownloadItemAndContentInChangesAsync(Commit commit)
        {
            var tasks = from change in commit.Changes
                        where ChangeTypeHasContent(change)
                        select DownloadItemAndContentInCommitAsync(change, commit);
            await Task.WhenAll(tasks);
        }

        private static bool ChangeTypeHasContent(ItemChange change)
        {
            return (change.ChangeType & (ItemChangeType.Edit | ItemChangeType.Add | ItemChangeType.Rename)) != 0;
        }

        private async Task DownloadItemAndContentInCommitAsync(ItemChange change, Commit commit)
        {
            var item = await _httpClient.GetItemAsync(_push.Repository.Id, change.Item.Path,
                                includeContentMetadata: true,
                                versionDescriptor: new Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionDescriptor()
                                {
                                    VersionType = Microsoft.TeamFoundation.SourceControl.WebApi.GitVersionType.Commit,
                                    Version = commit.CommitId
                                },
                                cancellationToken: _token);
            change.Item.ContentMetadata = item.ContentMetadata.ToSync();

            var content = await _httpClient.GetBlobContentAsync(_push.Repository.Id, item.ObjectId,
                                cancellationToken: _token);
            change.NewContent = await change.CreateItemContentAsync(content, _token);
        }
    }
}
