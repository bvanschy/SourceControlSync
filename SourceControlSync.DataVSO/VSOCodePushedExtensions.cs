using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO
{
    public static class VSOCodePushedExtensions
    {
        public static Push ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitPush push)
        {
            if (push == null)
            {
                throw new ArgumentNullException("push");
            }
            return new Push()
            {
                Commits = push.Commits != null ? push.Commits.ToSync() : null,
                Repository = push.Repository != null ? push.Repository.ToSync() : null
            };
        }

        public static IEnumerable<Commit> ToSync(this IEnumerable<Microsoft.TeamFoundation.SourceControl.WebApi.GitCommitRef> commits)
        {
            return commits.Select(c => new Commit()
                {
                    Changes = c.Changes != null ? c.Changes.ToSync() : null,
                    CommitId = c.CommitId,
                    Committer = c.Committer != null ? c.Committer.ToSync() : null
                }).ToArray();
        }

        public static IEnumerable<ItemChange> ToSync(this IEnumerable<Microsoft.TeamFoundation.SourceControl.WebApi.GitChange> changes)
        {
            return changes.Select(c => new ItemChange()
                {
                    ChangeType = c.ChangeType.ToSync(),
                    Item = c.Item != null ? c.Item.ToSync() : null
                }).ToArray();
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
            return new Item()
            {
                Path = item.Path
            };
        }

        public static bool IsInRoot(this Microsoft.TeamFoundation.SourceControl.WebApi.GitItem item, string root)
        {
            return item.Path.StartsWith(root);
        }

        public static FileContentMetadata ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.FileContentMetadata metadata)
        {
            var newMetadata = new FileContentMetadata()
            {
                ContentType = metadata.ContentType,
                IsBinary = metadata.IsBinary
            };
            if (!metadata.IsBinary)
            {
                newMetadata.Encoding = Encoding.GetEncoding(metadata.Encoding);
            }
            return newMetadata;
        }

        public static UserDate ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitUserDate userDate)
        {
            return new UserDate()
            {
                Date = userDate.Date
            };
        }

        public static Repository ToSync(this Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository repo)
        {
            return new Repository()
            {
                Id = repo.Id
            };
        }
    }
}
