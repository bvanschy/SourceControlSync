using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO.Tests
{
    [TestClass]
    public class DownloadRequestTests
    {
        [TestMethod]
        [Ignore]
        public void GetChanges()
        {
            var repo = CreateDownloadRequest();
            var commit = new Commit() { CommitId = "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5" };

            repo.DownloadChangesInCommitAsync(
                commit, Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), 
                CancellationToken.None).Wait();

            Assert.IsNotNull(commit.Changes);
            Assert.AreEqual(1, commit.Changes.Count());
            Assert.AreEqual(ItemChangeType.Add, commit.Changes.Single().ChangeType);
            Assert.IsNotNull(commit.Changes.Single().Item);
            Assert.AreEqual("/index.html", commit.Changes.Single().Item.Path);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(TaskCanceledException))]
        public void GetChangesWhenCanceled()
        {
            var repo = CreateDownloadRequest();

            var task = repo.DownloadChangesInCommitAsync(
                new Commit() { CommitId = "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5" },
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), 
                new CancellationToken(true));

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        [TestMethod]
        [Ignore]
        public void GetTextItemAndBlob()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange() { Item = new Item() { Path = "/index.html" } };

            repo.DownloadItemAndContentInCommitAsync(change,
                new Commit() { CommitId = "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5" },
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"),
                CancellationToken.None).Wait();

            Assert.IsNotNull(change.Item.ContentMetadata);
            Assert.AreEqual("text/html", change.Item.ContentMetadata.ContentType);
            Assert.AreEqual(65001, change.Item.ContentMetadata.Encoding.CodePage);
            Assert.IsFalse(change.Item.ContentMetadata.IsBinary);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.RawText, change.NewContent.ContentType);
            Assert.AreEqual(119, change.NewContent.Content.Length);
        }

        [TestMethod]
        [Ignore]
        public void GetBinaryItemAndBlob()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange() { Item = new Item() { Path = "/favicon.ico" } };

            repo.DownloadItemAndContentInCommitAsync(change,
                new Commit() { CommitId = "b6f447775f71a092854a2555eea084bd6d19958e" },
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"),
                CancellationToken.None).Wait();

            Assert.IsNotNull(change.Item.ContentMetadata);
            Assert.AreEqual("image/x-icon", change.Item.ContentMetadata.ContentType);
            Assert.IsTrue(change.Item.ContentMetadata.IsBinary);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.Base64Encoded, change.NewContent.ContentType);
            Assert.AreEqual(1536, change.NewContent.Content.Length);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(TaskCanceledException))]
        public void GetTextItemAndBlobWhenCanceled()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange() { Item = new Item() { Path = "/index.html" } };

            var task = repo.DownloadItemAndContentInCommitAsync(change,
                new Commit() { CommitId = "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5" },
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"),
                new CancellationToken(true));

            try
            {
                task.Wait();
            }
            catch (AggregateException e)
            {
                throw e.InnerException;
            }
        }

        private static IDownloadRequest CreateDownloadRequest()
        {
            var connectionStringBuilder = new VSOConnectionStringBuilder()
            {
                BaseUrl = new Uri(ConfigurationManager.AppSettings["VSO-BaseUrl"]),
                Credentials = new Credentials()
                {
                    UserName = ConfigurationManager.AppSettings["VSO-UserName"],
                    AccessToken = ConfigurationManager.AppSettings["VSO-AccessToken"]
                }
            };
            var repo = new DownloadRequest(connectionStringBuilder.ConnectionString);
            return repo;
        }
    }
}
