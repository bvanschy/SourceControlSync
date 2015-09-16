using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class SourceRepositoryTests
    {
        [TestMethod]
        public void DownloadCommitWithChanges()
        {
            var commitIdRequest = "1b1859c414e800d24036b9ee547d1530431ae055";
            var pushRequest = CreatePushRequest(commitIdRequest);
            var fakeChanges = new ItemChange[] { new ItemChange(ItemChangeType.None, new Item("/index.html")) };
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    Assert.AreEqual(commitIdRequest, commitId);
                    return Task.FromResult(fakeChanges.AsEnumerable());
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(pushRequest, "/", CancellationToken.None).Wait();

            Assert.IsTrue(fakeChanges.SequenceEqual(pushRequest.Commits.Single().Changes));
        }

        [TestMethod]
        public void DownloadCommitWithAddedTextItem()
        {
            var push = CreatePushRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5");
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    var changes = new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Add, new Item("/index.html"))
                        };
                    return Task.FromResult(changes.AsEnumerable());
                },
                DownloadItemAndContentInCommitAsyncItemChangeStringGuidCancellationToken = (itemChange, commitId, repositoryId, token) =>
                {
                    itemChange.Item.ContentMetadata = CreateTextContentMetadataTestData();
                    itemChange.NewContent = CreateTextContentTestData();
                    return Task.FromResult(0);
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/", CancellationToken.None).Wait();

            var change = push.Commits.Single().Changes.Single();
            Assert.AreEqual(ItemChangeType.Add, change.ChangeType);
            Assert.AreEqual("/index.html", change.Item.Path);
            Assert.IsNotNull(change.Item.ContentMetadata);
            Assert.IsFalse(change.Item.ContentMetadata.IsBinary);
            Assert.AreEqual("text/html", change.Item.ContentMetadata.ContentType);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.RawText, change.NewContent.ContentType);
            Assert.AreEqual("Testing", change.NewContent.Content);
        }

        [TestMethod]
        public void DownloadCommitWithRenamedTextItem()
        {
            var push = CreatePushRequest("de3e7a550c40fe75085d11e81d5770bc5b0dd33c");
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    var changes = new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Rename, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Delete | ItemChangeType.SourceRename, new Item("/index2.html"))
                        };
                    return Task.FromResult(changes.AsEnumerable());
                },
                DownloadItemAndContentInCommitAsyncItemChangeStringGuidCancellationToken = (itemChange, commitId, repositoryId, token) =>
                {
                    if (itemChange.Item.Path == "/index.html")
                    {
                        itemChange.Item.ContentMetadata = CreateTextContentMetadataTestData();
                        itemChange.NewContent = CreateTextContentTestData();
                    }
                    return Task.FromResult(0);
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/", CancellationToken.None).Wait();

            var index2Item = push.Commits.Single().Changes.Single(c => c.Item.Path == "/index2.html");
            Assert.AreEqual(ItemChangeType.Delete | ItemChangeType.SourceRename, index2Item.ChangeType);

            var indexItem = push.Commits.Single().Changes.Single(c => c.Item.Path == "/index.html");
            Assert.AreEqual(ItemChangeType.Rename, indexItem.ChangeType);
            Assert.IsNotNull(indexItem.Item.ContentMetadata);
            Assert.IsFalse(indexItem.Item.ContentMetadata.IsBinary);
            Assert.AreEqual("text/html", indexItem.Item.ContentMetadata.ContentType);
            Assert.IsNotNull(indexItem.NewContent);
            Assert.AreEqual(ItemContentType.RawText, indexItem.NewContent.ContentType);
            Assert.AreEqual("Testing", indexItem.NewContent.Content);
        }

        [TestMethod]
        public void DownloadCommitWithRenamedAndEditedItem()
        {
            var push = CreatePushRequest("a620293e7300c85234c5109e9cd9bb056942fbd6");
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    var changes = new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Delete | ItemChangeType.SourceRename, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Edit | ItemChangeType.Rename, new Item("/index3.html"))
                        };
                    return Task.FromResult(changes.AsEnumerable());
                },
                DownloadItemAndContentInCommitAsyncItemChangeStringGuidCancellationToken = (itemChange, commitId, repositoryId, token) =>
                {
                    if (itemChange.Item.Path == "/index3.html")
                    {
                        itemChange.Item.ContentMetadata = CreateTextContentMetadataTestData();
                        itemChange.NewContent = CreateTextContentTestData();
                    }
                    return Task.FromResult(0);
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/", CancellationToken.None).Wait();

            var indexItem = push.Commits.Single().Changes.Single(c => c.Item.Path == "/index.html");
            Assert.AreEqual(ItemChangeType.Delete | ItemChangeType.SourceRename, indexItem.ChangeType);

            var index3Item = push.Commits.Single().Changes.Single(c => c.Item.Path == "/index3.html");
            Assert.AreEqual(ItemChangeType.Edit | ItemChangeType.Rename, index3Item.ChangeType);
            Assert.IsNotNull(index3Item.Item.ContentMetadata);
            Assert.IsFalse(index3Item.Item.ContentMetadata.IsBinary);
            Assert.AreEqual("text/html", index3Item.Item.ContentMetadata.ContentType);
            Assert.IsNotNull(index3Item.NewContent);
            Assert.AreEqual(ItemContentType.RawText, index3Item.NewContent.ContentType);
            Assert.AreEqual("Testing", index3Item.NewContent.Content);
        }

        [TestMethod]
        public void DownloadCommitWithAddedBinaryItem()
        {
            var push = CreatePushRequest("b6f447775f71a092854a2555eea084bd6d19958e");
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    var changes = new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Add, new Item("/favicon.ico"))
                        };
                    return Task.FromResult(changes.AsEnumerable());
                },
                DownloadItemAndContentInCommitAsyncItemChangeStringGuidCancellationToken = (itemChange, commitId, repositoryId, token) =>
                {
                    itemChange.Item.ContentMetadata = CreateBinaryContentMetadataTestData();
                    itemChange.NewContent = CreateBinaryContentTestData();
                    return Task.FromResult(0);
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/", CancellationToken.None).Wait();

            var change = push.Commits.Single().Changes.Single();
            Assert.AreEqual(ItemChangeType.Add, change.ChangeType);
            Assert.AreEqual("/favicon.ico", change.Item.Path);
            Assert.IsTrue(change.Item.ContentMetadata.IsBinary);
            Assert.AreEqual("image/x-icon", change.Item.ContentMetadata.ContentType);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.Base64Encoded, change.NewContent.ContentType);
            Assert.AreEqual(12, change.NewContent.Content.Length);
        }

        [TestMethod]
        public void DownloadAllCommits()
        {
            var commits = new Dictionary<string, string>()
            {
                { "fd29ca5fdf9e938c873b29fd1e074aea913b831b", "2015-08-16T15:57:18Z" },
                { "b6f447775f71a092854a2555eea084bd6d19958e", "2015-08-16T05:07:59Z" },
                { "a620293e7300c85234c5109e9cd9bb056942fbd6", "2015-08-16T04:59:10Z" },
                { "de3e7a550c40fe75085d11e81d5770bc5b0dd33c", "2015-08-16T04:39:20Z" },
                { "be993da1b6b79d0a9361b89fd980000ca7f03823", "2015-08-16T04:31:57Z" },
                { "1b1859c414e800d24036b9ee547d1530431ae055", "2015-08-16T04:29:45Z" },
                { "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z" }
            };
            var push = CreatePushRequest(commits);
            var changesByCommit = GetChangesTestData();
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commitId, repositoryId, token) =>
                {
                    var changes = changesByCommit[commitId];
                    return Task.FromResult(changes.AsEnumerable());
                },
                DownloadItemAndContentInCommitAsyncItemChangeStringGuidCancellationToken = (itemChange, commitId, repositoryId, token) =>
                {
                    return Task.FromResult(0);
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/", CancellationToken.None).Wait();

            Assert.AreEqual(10, push.Commits.Sum(c => c.Changes.Count()));
        }

        [TestMethod]
        public void DownloadNoChangesInRoot()
        {
            var push = CreatePushRequest("1b1859c414e800d24036b9ee547d1530431ae055");
            var fakeDownloadRequest = new Fakes.StubIDownloadRequest()
            {
                DownloadChangesInCommitAsyncStringGuidCancellationToken = (commit, repositoryId, token) =>
                {
                    var changes = new ItemChange[] 
                        { 
                            new ItemChange(ItemChangeType.Edit, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Add, new Item("/index2.html"))
                        };
                    return Task.FromResult(changes.AsEnumerable());
                }
            };
            var repo = new SourceRepository(fakeDownloadRequest);

            repo.DownloadChangesAsync(push, "/fake/", CancellationToken.None).Wait();

            Assert.AreEqual(0, push.Commits.Single().Changes.Count());
        }

        private static Push CreatePushRequest(string commitId)
        {
            var push = new Push(
                new Repository(Guid.NewGuid()), 
                new Commit[] { new Commit(commitId, new UserDate(DateTime.Now)) }
                );
            return push;
        }

        private static Push CreatePushRequest(Dictionary<string, string> commitIdsAndDates)
        {
            var commits = commitIdsAndDates.Select(c => CreateCommit(c));
            var push = new Push(new Repository(Guid.NewGuid()), commits);
            return push;
        }

        private static Commit CreateCommit(KeyValuePair<string, string> kvp)
        {
            return new Commit(
                kvp.Key, 
                new UserDate(DateTime.ParseExact(kvp.Value, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture))
                );
        }

        private static IDictionary<string, IEnumerable<ItemChange>> GetChangesTestData()
        {
            return new Dictionary<string, IEnumerable<ItemChange>>()
                {
                    {
                        "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", 
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Add, new Item("/index.html"))
                        }
                    },
                    { 
                        "1b1859c414e800d24036b9ee547d1530431ae055", 
                        new ItemChange[] 
                        { 
                            new ItemChange(ItemChangeType.Edit, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Add, new Item("/index2.html"))
                        }
                    },
                    {
                        "be993da1b6b79d0a9361b89fd980000ca7f03823",
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Delete, new Item("/index.html"))
                        }
                    },
                    {
                        "de3e7a550c40fe75085d11e81d5770bc5b0dd33c",
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Rename, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Delete | ItemChangeType.SourceRename, new Item("/index2.html"))
                        }
                    },
                    {
                        "a620293e7300c85234c5109e9cd9bb056942fbd6",
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Delete | ItemChangeType.SourceRename, new Item("/index.html")),
                            new ItemChange(ItemChangeType.Edit | ItemChangeType.Rename, new Item("/index3.html"))
                        }
                    },
                    {
                        "b6f447775f71a092854a2555eea084bd6d19958e",
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Add, new Item("/favicon.ico"))
                        }
                    },
                    {
                        "fd29ca5fdf9e938c873b29fd1e074aea913b831b",
                        new ItemChange[]
                        {
                            new ItemChange(ItemChangeType.Add, new Item("/index4.ico"))
                        }
                    }
                };
        }

        private static FileContentMetadata CreateTextContentMetadataTestData()
        {
            return new FileContentMetadata("text/html", Encoding.UTF8);
        }

        private static FileContentMetadata CreateBinaryContentMetadataTestData()
        {
            return new FileContentMetadata("image/x-icon");
        }

        private static ItemContent CreateTextContentTestData()
        {
            return new ItemContent(ItemContentType.RawText, "Testing");
        }

        private static ItemContent CreateBinaryContentTestData()
        {
            return new ItemContent(ItemContentType.Base64Encoded, Convert.ToBase64String(Encoding.UTF8.GetBytes("Testing")));
        }
    }
}
