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
        [TestMethod]
        public void Post()
        {
            var pushEvent = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");

            Push pushPassedToSource = null;
            string rootPassedToSource = null;
            IEnumerable<Commit> commitsPassedToCalculator = null;
            IEnumerable<ItemChange> changesReturnedByCalculator = null;
            IEnumerable<ItemChange> changesPassedToDestination = null;
            string rootPassedToDestination = null;
            #region Fakes
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

            var fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator()
            {
                CalculateItemChangesIEnumerableOfCommit = (commits) => 
                {
                    commitsPassedToCalculator = commits;
                    changesReturnedByCalculator = commits.SelectMany(commit => commit.Changes).ToList();
                },
                ItemChangesGet = () => { return changesReturnedByCalculator; }
            };

            var fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository()
            {
                PushItemChangesAsyncIEnumerableOfItemChangeString = (changes, root) =>
                {
                    changesPassedToDestination = changes;
                    rootPassedToDestination = root;
                    return Task.FromResult(0);
                }
            };

            var fakeChangesReport = new SourceControlSync.Domain.Fakes.StubIChangesReport();
            var fakeErrorReport = new SourceControlSync.Domain.Fakes.StubIErrorReport();
            #endregion

            using (var controller = CreateVSOController(
                fakeSourceRepository, 
                fakeDestinationRepository, 
                fakeChangesCalculator, 
                fakeChangesReport, 
                fakeErrorReport
                ))
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

            IExecutedCommands reportExecutedCommands = null;
            Exception reportException = null;
            string reportRequest = null;
            #region Fakes
            var fakeSourceRepository = new SourceControlSync.Domain.Fakes.StubISourceRepository()
            {
                DownloadChangesAsyncPushStringCancellationToken = (push, root, token) => { return Task.FromResult(0); }
            };
            var fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator();
            var fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository()
            {
                PushItemChangesAsyncIEnumerableOfItemChangeString = (changes, root) => { return Task.FromResult(0); },
                ExecutedCommandsGet = () => { return new SourceControlSync.Domain.Fakes.StubIExecutedCommands(); }
            };
            IChangesReport fakeChangesReport = new SourceControlSync.Domain.Fakes.StubIChangesReport()
            {
                ExecutedCommandsSetIExecutedCommands = (commands) => { reportExecutedCommands = commands; },
                ExecutedCommandsGet = () => { return reportExecutedCommands; },
                ExceptionSetException = (ex) => { reportException = ex; },
                ExceptionGet = () => { return reportException; },
                RequestSetString = (evt) => { reportRequest = evt; },
                RequestGet = () => { return reportRequest; },
                HasMessageGet = () => { return true; },
                ToMailMessage = () => { return new System.Net.Mail.MailMessage(); }
            };
            IErrorReport fakeErrorReport = new SourceControlSync.Domain.Fakes.StubIErrorReport();
            #endregion

            using (var controller = CreateVSOController(
                fakeSourceRepository, 
                fakeDestinationRepository, 
                fakeChangesCalculator, 
                fakeChangesReport, 
                fakeErrorReport
                ))
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

            IExecutedCommands reportExecutedCommands = null;
            Exception reportException = null;
            string reportRequest = null;
            #region Fakes
            var fakeSourceRepository = new SourceControlSync.Domain.Fakes.StubISourceRepository()
            {
                DownloadChangesAsyncPushStringCancellationToken = (push, root, token) => { throw new Exception("Oops!"); }
            };
            var fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator();
            var fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository();

            IChangesReport fakeChangesReport = new SourceControlSync.Domain.Fakes.StubIChangesReport()
            {
                ExecutedCommandsSetIExecutedCommands = (commands) => { reportExecutedCommands = commands; },
                ExecutedCommandsGet = () => { return reportExecutedCommands; },
                ExceptionSetException = (ex) => { reportException = ex; },
                ExceptionGet = () => { return reportException; },
                RequestSetString = (evt) => { reportRequest = evt; },
                RequestGet = () => { return reportRequest; },
                HasMessageGet = () => { return true; },
                ToMailMessage = () => { return new System.Net.Mail.MailMessage(); }
            };
            IErrorReport fakeErrorReport = new SourceControlSync.Domain.Fakes.StubIErrorReport();
            #endregion

            using (var controller = CreateVSOController(
                fakeSourceRepository, 
                fakeDestinationRepository, 
                fakeChangesCalculator, 
                fakeChangesReport, 
                fakeErrorReport
                ))
            {
                var result = controller.PostAsync(pushEvent, CancellationToken.None).Result;

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

            Exception reportException = null;
            string reportRequest = null;
            #region Fakes
            var fakeSourceRepository = new SourceControlSync.Domain.Fakes.StubISourceRepository()
            {
                DownloadChangesAsyncPushStringCancellationToken = (push, root, token) => { throw new Exception("Oops!"); }
            };
            var fakeChangesCalculator = new SourceControlSync.Domain.Fakes.StubIChangesCalculator();
            var fakeDestinationRepository = new SourceControlSync.Domain.Fakes.StubIDestinationRepository();
            IChangesReport fakeChangesReport = new SourceControlSync.Domain.Fakes.StubIChangesReport();
            IErrorReport fakeErrorReport = new SourceControlSync.Domain.Fakes.StubIErrorReport()
            {
                ExceptionSetException = (ex) => { reportException = ex; },
                ExceptionGet = () => { return reportException; },
                RequestSetString = (evt) => { reportRequest = evt; },
                RequestGet = () => { return reportRequest; },
                HasMessageGet = () => { return reportException != null; },
                ToMailMessage = () => { return new System.Net.Mail.MailMessage(); }
            };
            #endregion

            using (var controller = CreateVSOController(
                fakeSourceRepository, 
                fakeDestinationRepository, 
                fakeChangesCalculator, 
                fakeChangesReport, 
                fakeErrorReport
                ))
            {
                var result = controller.PostAsync(pushEvent, CancellationToken.None).Result;

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

        private static VSOController CreateVSOController(
            ISourceRepository sourceRepository, 
            IDestinationRepository destinationRepository,
            IChangesCalculator changesCalculator, 
            IChangesReport changesReport, 
            IErrorReport errorReport)
        {
            var fakeSourceRepositoryFactory = new SourceControlSync.Domain.Fakes.StubISourceRepositoryFactory()
            {
                CreateSourceRepositoryString = (connectionString) => { return sourceRepository; }
            };
            var fakeDestinationRepositoryFactory = new SourceControlSync.Domain.Fakes.StubIDestinationRepositoryFactory()
            {
                CreateDestinationRepositoryString = (connectionString) => { return destinationRepository; }
            };
            var controller = new VSOController(
                fakeSourceRepositoryFactory, 
                fakeDestinationRepositoryFactory, 
                changesCalculator, 
                changesReport, 
                errorReport
                );
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "");
            controller.Request.Headers.Add(VSOController.HEADER_SOURCE_CONNECTIONSTRING, "");
            controller.Request.Headers.Add(VSOController.HEADER_DESTINATION_CONNECTIONSTRING, "");
            controller.Request.Headers.Add(VSOController.HEADER_ROOT, "/");
            return controller;
        }
    }
}
