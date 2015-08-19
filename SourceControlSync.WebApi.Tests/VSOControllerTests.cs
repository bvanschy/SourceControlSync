using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.WebApi.Controllers;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using SourceControlSync.DataVSO;
using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace SourceControlSync.WebApi.Tests
{
    [TestClass]
    public class VSOControllerTests
    {
        [TestMethod]
        public void VSOControllerPost()
        {
            var push = CreateVSOCodePushedRequest(commitId: "5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", commitDate: "2015-08-16T04:28:13Z");

            var fakeSourceRepository = new FakeSourceRepository();
            var fakeChangesCalculator = new FakeChangesCalculator();
            var fakeDestinationRepository = new FakeDestinationRepository();
            var controller = new VSOController(fakeSourceRepository, fakeChangesCalculator, fakeDestinationRepository);
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "");
            controller.Request.Headers.Add(VSOController.HEADER_VSO_BASE_URL, "VSO-BaseUrl");
            controller.Request.Headers.Add(VSOController.HEADER_VSO_USER_NAME, "VSO-UserName");
            controller.Request.Headers.Add(VSOController.HEADER_VSO_ACCESS_TOKEN, "VSO-AccessToken");
            controller.Request.Headers.Add(VSOController.HEADER_VSO_ROOT, "/");
            controller.Request.Headers.Add(VSOController.HEADER_AWS_ACCESS_KEY_ID, "AWS-AccessKeyId");
            controller.Request.Headers.Add(VSOController.HEADER_AWS_SECRET_ACCESS_KEY, "AWS-SecretAccessKey");
            controller.Request.Headers.Add(VSOController.HEADER_AWS_REGION_SYSTEM_NAME, "AWS-SystemName");
            controller.Request.Headers.Add(VSOController.HEADER_AWS_BUCKET_NAME, "AWS-BucketName");

            var result = controller.Post(push).Result;

            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), fakeSourceRepository.PushPassed.Repository.Id);
            Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", fakeSourceRepository.PushPassed.Commits.Single().CommitId);
            Assert.AreEqual("/", fakeSourceRepository.RootPassed);
            Assert.IsNotNull(fakeSourceRepository.PushPassed.Commits.Single().Changes);
            Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", fakeChangesCalculator.CommitsPassed.Single().CommitId);
            Assert.AreSame(fakeChangesCalculator.ChangesReturned, fakeDestinationRepository.ChangesPassed);
            Assert.AreEqual("/", fakeDestinationRepository.RootPassed);
        }

        private static VSOCodePushed CreateVSOCodePushedRequest(string commitId, string commitDate)
        {
            var push = new VSOCodePushed()
            {
                Resource = new GitPush()
                {
                    Commits = new GitCommit[] 
                    {  
                        new GitCommit()
                        {
                            CommitId = commitId,
                            Committer = new GitUserDate() { Date = DateTime.ParseExact(commitDate, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) }
                        }
                    },
                    Repository = new GitRepository() { Id = new Guid("0ad49569-db8b-4a8a-b5cc-f7ff009949c8") }
                }
            };
            return push;
        }

        private class FakeSourceRepository : ISourceRepository
        {
            public Push PushPassed { get; set; }
            public string RootPassed { get; set; }

            public Task DownloadChangesAsync(Push push, string root = null)
            {
                PushPassed = push;
                RootPassed = root;

                foreach (var commit in push.Commits)
                {
                    commit.Changes = Enumerable.Empty<ItemChange>();
                }

                return Task.FromResult(0);
            }
        }

        private class FakeChangesCalculator : IChangesCalculator
        {
            public IEnumerable<Commit> CommitsPassed { get; set; }
            public IEnumerable<ItemChange> ChangesReturned { get; set; }

            public IEnumerable<ItemChange> CalculateItemChanges(IEnumerable<Commit> commits)
            {
                CommitsPassed = commits;
                ChangesReturned = commits.SelectMany(c => c.Changes).ToList();
                return ChangesReturned;
            }
        }

        private class FakeDestinationRepository : IDestinationRepository
        {
            public IEnumerable<ItemChange> ChangesPassed { get; set; }
            public string RootPassed { get; set; }

            public Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root)
            {
                ChangesPassed = changes;
                RootPassed = root;
                return Task.FromResult(0);
            }
        }
    }
}
