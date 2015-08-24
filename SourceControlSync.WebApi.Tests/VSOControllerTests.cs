﻿using Microsoft.TeamFoundation.SourceControl.WebApi;
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
        public void VSOControllerPost()
        {
            var push = CreateVSOCodePushedRequest("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", "2015-08-16T04:28:13Z");
            var fakeSourceRepository = new FakeSourceRepository();
            var fakeChangesCalculator = new FakeChangesCalculator();
            var fakeDestinationRepository = new FakeDestinationRepository();
            var controller = new VSOController()
            {
                SourceRepository = fakeSourceRepository,
                ChangesCalculator = fakeChangesCalculator,
                DestinationRepository = fakeDestinationRepository
            };
            controller.Request = new HttpRequestMessage(HttpMethod.Post, "");
            controller.Request.Headers.Add(VSOController.HEADER_SOURCE_CONNECTIONSTRING, "SourceConnectioniString");
            controller.Request.Headers.Add(VSOController.HEADER_DESTINATION_CONNECTIONSTRING, "DestinationConnectionString");
            controller.Request.Headers.Add(VSOController.HEADER_ROOT, "/");

            var result = controller.PostAsync(push, CancellationToken.None).Result;

            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(Guid.Parse("0ad49569-db8b-4a8a-b5cc-f7ff009949c8"), fakeSourceRepository.PushPassed.Repository.Id);
            Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", fakeSourceRepository.PushPassed.Commits.Single().CommitId);
            Assert.AreEqual("/", fakeSourceRepository.RootPassed);
            Assert.IsNotNull(fakeSourceRepository.PushPassed.Commits.Single().Changes);

            Assert.AreEqual("5597f65ce55386a771e4bf6fa190b5a26c0f5ce5", fakeChangesCalculator.CommitsPassed.Single().CommitId);

            Assert.AreSame(fakeChangesCalculator.ItemChanges, fakeDestinationRepository.ChangesPassed);
            Assert.AreEqual("/", fakeDestinationRepository.RootPassed);
        }

        [TestMethod]
        public void VSOControllerPostNoHeaders()
        {
            var controller = new VSOController();
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

        private class FakeSourceRepository : ISourceRepository
        {
            public Push PushPassed { get; set; }
            public string RootPassed { get; set; }

            public string ConnectionString { get; set; }

            public Task DownloadChangesAsync(Push push, string root, CancellationToken token)
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
            public IEnumerable<ItemChange> ItemChanges { get; set; }

            public void CalculateItemChanges(IEnumerable<Commit> commits)
            {
                CommitsPassed = commits;
                ItemChanges = commits.SelectMany(c => c.Changes).ToList();
            }
        }

        private class FakeDestinationRepository : IDestinationRepository
        {
            public IEnumerable<ItemChange> ChangesPassed { get; set; }
            public string RootPassed { get; set; }

            public string ConnectionString { get; set; }

            public Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root)
            {
                ChangesPassed = changes;
                RootPassed = root;
                return Task.FromResult(0);
            }
        }
    }
}