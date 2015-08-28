using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain.Models;
using SourceControlSync.WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;

namespace SourceControlSync.WebApi.Tests
{
    [TestClass]
    public class VSOControllerTests
    {
        [TestMethod]
        public void VSOControllerPost()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            Push pushPassedToSource = null;
            string rootPassedToSource = null;
            var fakeSourceRepository = new SourceControlSync.Domain.Fakes.StubISourceRepository()
            {
                DownloadChangesAsyncPushStringCancellationToken = (push, root, token) =>
                {
                    pushPassedToSource = push;
                    rootPassedToSource = root;
                    foreach (var commit in push.Commits)
                    {
                        commit.Changes = Enumerable.Empty<ItemChange>();
                    }
                    return Task.FromResult(0);
                }
            };

            IEnumerable<Commit> commitsPassedToCalculator = null;
            IEnumerable<ItemChange> changesReturnedByCalculator = null;
            var fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator()
            {
                CalculateItemChangesIEnumerableOfCommit = (commits) => 
                {
                    commitsPassedToCalculator = commits;
                    changesReturnedByCalculator = commits.SelectMany(commit => commit.Changes).ToList();
                },
                ItemChangesGet = () => { return changesReturnedByCalculator; }
            };

            IEnumerable<ItemChange> changesPassedToDestination = null;
            string rootPassedToDestination = null;
            var fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository()
            {
                PushItemChangesAsyncIEnumerableOfItemChangeString = (changes, root) =>
                {
                    changesPassedToDestination = changes;
                    rootPassedToDestination = root;
                    return Task.FromResult(0);
                }
            };

            var fakeRepositoryFactory = new SourceControlSync.Domain.Fakes.StubIRepositoryFactory()
            {
                CreateSourceRepositoryString = (connectionString) => { return fakeSourceRepository; },
                CreateDestinationRepositoryString = (connectionString) => { return fakeDestinationRepository; }
            };

            using (var controller = new VSOController(fakeRepositoryFactory, fakeChangesCalculator, null))
            {
                controller.Request = new HttpRequestMessage(HttpMethod.Post, "");
                controller.Request.Headers.Add(VSOController.HEADER_SOURCE_CONNECTIONSTRING, "SourceConnectioniString");
                controller.Request.Headers.Add(VSOController.HEADER_DESTINATION_CONNECTIONSTRING, "DestinationConnectionString");
                controller.Request.Headers.Add(VSOController.HEADER_ROOT, "/");

                var result = controller.PostAsync(pushEvent, CancellationToken.None).Result;

                Assert.IsInstanceOfType(result, typeof(OkResult));
                Assert.AreEqual(Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), pushPassedToSource.Repository.Id);
                Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", pushPassedToSource.Commits.Single().CommitId);
                Assert.AreEqual("/", rootPassedToSource);
                Assert.IsNotNull(pushPassedToSource.Commits.Single().Changes);

                Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", commitsPassedToCalculator.Single().CommitId);

                Assert.AreSame(changesReturnedByCalculator, changesPassedToDestination);
                Assert.AreEqual("/", rootPassedToDestination);
            }
        }

        [TestMethod]
        public void VSOControllerPostNoHeaders()
        {
            var controller = new VSOController(null, null, null);
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "");

            var result = controller.PostAsync(null, CancellationToken.None).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
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
                            Committer = new GitUserDate() 
                            { 
                                Date = DateTime.ParseExact(commitDate, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture) 
                            }
                        }
                    },
                    Repository = new GitRepository() { Id = new Guid("0ad49569-db8b-4a8a-b5cc-f7ff009949c8") }
                }
            };
            return push;
        }
    }
}
