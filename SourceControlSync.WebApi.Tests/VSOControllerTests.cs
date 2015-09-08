using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
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
        private Push pushPassedToSource;
        private string rootPassedToSource;
        private ISourceRepository fakeSourceRepository;

        private IEnumerable<Commit> commitsPassedToCalculator;
        private IEnumerable<ItemChange> changesReturnedByCalculator;
        private IChangesCalculator fakeChangesCalculator;

        private IEnumerable<ItemChange> changesPassedToDestination;
        private string rootPassedToDestination;
        private IDestinationRepository fakeDestinationRepository;

        private IChangesReport fakeChangesReport;
        private IErrorReport fakeErrorReport;

        [TestInitialize]
        public void InitializeTest()
        {
            pushPassedToSource = null;
            rootPassedToSource = null;
            fakeSourceRepository = new SourceControlSync.Domain.Fakes.StubISourceRepository()
            {
                DownloadChangesAsyncPushStringCancellationToken = (push, root, token) =>
                {
                    token.ThrowIfCancellationRequested();
                    pushPassedToSource = push;
                    rootPassedToSource = root;
                    foreach (var commit in push.Commits)
                    {
                        commit.Changes = Enumerable.Empty<ItemChange>();
                    }
                    return Task.FromResult(0);
                }
            };

            commitsPassedToCalculator = null;
            changesReturnedByCalculator = null;
            fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator()
            {
                CalculateItemChangesIEnumerableOfCommit = (commits) =>
                {
                    commitsPassedToCalculator = commits;
                    changesReturnedByCalculator = commits.SelectMany(commit => commit.Changes).ToList();
                },
                ItemChangesGet = () => { return changesReturnedByCalculator; }
            };

            changesPassedToDestination = null;
            rootPassedToDestination = null;
            fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository()
            {
                PushItemChangesAsyncIEnumerableOfItemChangeString = (changes, root) =>
                {
                    changesPassedToDestination = changes;
                    rootPassedToDestination = root;
                    return Task.FromResult(0);
                },
                ExecutedCommandsGet = () => { return new SourceControlSync.Domain.Fakes.StubIExecutedCommands(); }
            };

            var fakeClock = new SourceControlSync.Domain.Fakes.StubIClock();
            fakeChangesReport = new ChangesReport(fakeClock);
            fakeErrorReport = new ErrorReport(fakeClock);
        }

        [TestMethod]
        public void Post()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            using (var controller = CreateVSOController())
            {
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
        public void PostNoHeaders()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");
            using (var controller = new VSOController(null, null, null, null, null))
            {
                controller.Request = new HttpRequestMessage(HttpMethod.Post, "");

                var result = controller.PostAsync(pushEvent, CancellationToken.None).Result;

                Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            }
        }

        [TestMethod]
        public void ChangesReportFromPost()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            using (var controller = CreateVSOController())
            {
                var result = controller.PostAsync(pushEvent, CancellationToken.None).Result;

                Assert.IsInstanceOfType(result, typeof(OkResult));
                Assert.IsNotNull(fakeChangesReport.Request);
                Assert.IsTrue(fakeChangesReport.Request.Contains("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5"));
                Assert.IsNotNull(fakeChangesReport.ExecutedCommands);
                Assert.IsNull(fakeChangesReport.Exception);
            }
        }

        [TestMethod]
        public void ChangesReportWithErrorFromPost()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            using (var controller = CreateVSOController())
            {
                var token = new CancellationToken(true);
                var result = controller.PostAsync(pushEvent, token).Result;

                Assert.IsInstanceOfType(result, typeof(ExceptionResult));
                Assert.IsNotNull(fakeChangesReport.Request);
                Assert.IsTrue(fakeChangesReport.Request.Contains("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5"));
                Assert.IsNull(fakeChangesReport.ExecutedCommands);
                Assert.IsNotNull(fakeChangesReport.Exception);
            }
        }

        [TestMethod]
        public void ErrorReportFromPost()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            using (var controller = CreateVSOController())
            {
                var token = new CancellationToken(true);
                var result = controller.PostAsync(pushEvent, token).Result;

                Assert.IsInstanceOfType(result, typeof(ExceptionResult));
                Assert.IsNotNull(fakeErrorReport.Request);
                Assert.IsTrue(fakeErrorReport.Request.Contains("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5"));
                Assert.IsNotNull(fakeErrorReport.Exception);
            }
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
                                Date = DateTime.ParseExact(commitDate, "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture),
                                Email = "me@example.com"
                            }
                        }
                    },
                    Repository = new GitRepository() { Id = new Guid("0ad49569-db8b-4a8a-b5cc-f7ff009949c8") }
                }
            };
            return push;
        }

        private VSOController CreateVSOController()
        {
            var fakeSourceRepositoryFactory = new SourceControlSync.Domain.Fakes.StubISourceRepositoryFactory()
            {
                CreateSourceRepositoryString = (connectionString) => { return fakeSourceRepository; }
            };
            var fakeDestinationRepositoryFactory = new SourceControlSync.Domain.Fakes.StubIDestinationRepositoryFactory()
            {
                CreateDestinationRepositoryString = (connectionString) => { return fakeDestinationRepository; }
            };
            var controller = new VSOController(
                fakeSourceRepositoryFactory, 
                fakeDestinationRepositoryFactory, 
                fakeChangesCalculator, 
                fakeChangesReport, 
                fakeErrorReport
                );
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "");
            controller.Request.Headers.Add(VSOController.HEADER_SOURCE_CONNECTIONSTRING, "");
            controller.Request.Headers.Add(VSOController.HEADER_DESTINATION_CONNECTIONSTRING, "");
            controller.Request.Headers.Add(VSOController.HEADER_ROOT, "/");
            return controller;
        }
    }
}
