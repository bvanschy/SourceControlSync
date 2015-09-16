using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    public static class TeamFoundationSourceControlExtensions
    {
        public static IList<string> GetCommitterEmails(this IEnumerable<Microsoft.TeamFoundation.SourceControl.WebApi.GitCommitRef> commits)
        {
            var recipients = (from commit in commits
                              where !string.IsNullOrWhiteSpace(commit.Committer.Email)
                              select commit.Committer.Email).Distinct().ToList();
            return recipients;
        }

        public static Push ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitPush push)
        {
            var repository = push.Repository != null ? push.Repository.ToSync() : null;
            var commits = push.Commits != null ? push.Commits.ToSync() : null;
            return new Push(repository, commits);
        }

        private static IEnumerable<Commit> ToSync(this IEnumerable<Microsoft.TeamFoundation.SourceControl.WebApi.GitCommitRef> commits)
        {
            return commits.Select(c => new Commit(c.CommitId, c.Committer != null ? c.Committer.ToSync() : null)
                {
                    Changes = c.Changes != null ? c.Changes.ToSync() : null
                });
        }

        public static IEnumerable<ItemChange> ToSync(this IEnumerable<Microsoft.TeamFoundation.SourceControl.WebApi.GitChange> changes)
        {
            return changes.Select(c => new ItemChange(c.ChangeType.ToSync(), c.Item != null ? c.Item.ToSync() : null));
        }

        public static ItemChangeType ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType changeType)
        {
            var itemChangeType = ItemChangeType.None;
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Add) != 0)
            {
                itemChangeType |= ItemChangeType.Add;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Edit) != 0)
            {
                itemChangeType |= ItemChangeType.Edit;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Encoding) != 0)
            {
                itemChangeType |= ItemChangeType.Encoding;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Rename) != 0)
            {
                itemChangeType |= ItemChangeType.Rename;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Delete) != 0)
            {
                itemChangeType |= ItemChangeType.Delete;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.Undelete) != 0)
            {
                itemChangeType |= ItemChangeType.Undelete;
            }
            if ((changeType & Microsoft.TeamFoundation.SourceControl.WebApi.VersionControlChangeType.SourceRename) != 0)
            {
                itemChangeType |= ItemChangeType.SourceRename;
            }
            return itemChangeType;
        }

        public static Item ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitItem item)
        {
            return new Item(item.Path);
        }

        public static FileContentMetadata ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.FileContentMetadata metadata)
        {
            if (metadata.IsBinary)
            {
                return new FileContentMetadata(metadata.ContentType);
            }
            else
            {
                return new FileContentMetadata(metadata.ContentType, Encoding.GetEncoding(metadata.Encoding));
            }
        }

        public static UserDate ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitUserDate userDate)
        {
            return new UserDate(userDate.Date);
        }

        public static Repository ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository repo)
        {
            return new Repository(repo.Id);
        }
    }
}
