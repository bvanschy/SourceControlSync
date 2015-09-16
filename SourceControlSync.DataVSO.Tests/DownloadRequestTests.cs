using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataVSO.Tests
{
    [TestClass]
    public class DownloadRequestTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GetChanges()
        {
            var repo = CreateDownloadRequest();

            var changes = repo.DownloadChangesInCommitAsync(
                "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", 
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), 
                CancellationToken.None
                ).Result;

            Assert.IsNotNull(changes);
            Assert.AreEqual(1, changes.Count());
            Assert.AreEqual(ItemChangeType.Add, changes.Single().ChangeType);
            Assert.IsNotNull(changes.Single().Item);
            Assert.AreEqual("/index.html", changes.Single().Item.Path);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public void GetChangesWhenCanceled()
        {
            var repo = CreateDownloadRequest();

            var task = repo.DownloadChangesInCommitAsync(
                "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5",
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), 
                new CancellationToken(true)
                );

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
        public void GetTextItemAndBlob()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange(ItemChangeType.Add, new Item("/index.html"));

            repo.DownloadItemAndContentInCommitAsync(
                change,
                "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5",
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"),
                CancellationToken.None
                ).Wait();

            Assert.IsNotNull(change.Item.ContentMetadata);
            Assert.AreEqual("text/html", change.Item.ContentMetadata.ContentType);
            Assert.AreEqual(65001, change.Item.ContentMetadata.Encoding.CodePage);
            Assert.IsFalse(change.Item.ContentMetadata.IsBinary);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.RawText, change.NewContent.ContentType);
            Assert.AreEqual(119, change.NewContent.Content.Length);
        }

        [TestMethod]
        public void GetBinaryItemAndBlob()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange(ItemChangeType.Add, new Item("/favicon.ico"));

            repo.DownloadItemAndContentInCommitAsync(
                change,
                "b6f447775f71a092854a2555eea084bd6d19958e",
                Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"),
                CancellationToken.None
                ).Wait();

            Assert.IsNotNull(change.Item.ContentMetadata);
            Assert.AreEqual("image/x-icon", change.Item.ContentMetadata.ContentType);
            Assert.IsTrue(change.Item.ContentMetadata.IsBinary);
            Assert.IsNotNull(change.NewContent);
            Assert.AreEqual(ItemContentType.Base64Encoded, change.NewContent.ContentType);
            Assert.AreEqual(1536, change.NewContent.Content.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public void GetTextItemAndBlobWhenCanceled()
        {
            var repo = CreateDownloadRequest();
            var change = new ItemChange(ItemChangeType.Add, new Item("/index.html"));

            var task = repo.DownloadItemAndContentInCommitAsync(
                change,
                "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5",
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

        private IDownloadRequest CreateDownloadRequest()
        {
            var baseUrl = TestContext.Properties["VSOBaseUrl"] as string;
            var credentials = new Credentials()
            {
                UserName = TestContext.Properties["VSOUserName"] as string,
                AccessToken = TestContext.Properties["VSOAccessToken"] as string
            };
            var repo = new DownloadRequest(baseUrl, credentials);
            return repo;
        }
    }
}
