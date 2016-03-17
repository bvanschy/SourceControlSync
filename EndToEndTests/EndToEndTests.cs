using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SourceControlSync.DataAWS;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using SourceControlSync.WebApi.Controllers;
using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Results;

namespace SourceControlSync.WebApi.Tests
{
    [TestClass]
    public class EndToEndTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Ignore]
        public void AddedFileInSubfolder()
        {
            var controller = CreateVSOController();
            controller.Request = CreateTestAgileRequest();

            var push = JsonConvert.DeserializeObject<VSOCodePushed>(SampleMessages.Push_AddedFileInSubfolder);

            var result = controller.PostAsync(push, CancellationToken.None).Result;

            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        private VSOController CreateVSOController()
        {
            return new VSOController(
                new VSORepositoryFactory(),
                new AWSS3RepositoryFactory(),
                new ChangesCalculator(),
                new ChangesReport(new Clock()),
                new ErrorReport(new Clock())
                );
        }

        private HttpRequestMessage CreateTestAgileRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "");

            var sourceConnectionString = string.Format(
                "BaseUrl={0};UserName={1};AccessToken={2}",
                TestContext.Properties["VSOBaseUrl"] as string,
                TestContext.Properties["VSOUserName"] as string,
                TestContext.Properties["VSOAccessToken"] as string
                );
            request.Headers.Add(VSOController.HEADER_SOURCE_CONNECTIONSTRING, sourceConnectionString);

            var destinationConnectionString = string.Format(
                "BucketName={0};RegionSystemName={1};AccessKeyId={2};SecretAccessKey={3}",
                TestContext.Properties["AWSBucketName"] as string,
                TestContext.Properties["AWSRegionSystemName"] as string,
                TestContext.Properties["AWSAccessKeyId"] as string,
                TestContext.Properties["AWSSecretAccessKey"] as string
                );
            request.Headers.Add(VSOController.HEADER_DESTINATION_CONNECTIONSTRING, destinationConnectionString);

            request.Headers.Add(VSOController.HEADER_ROOT, "/");

            return request;
        }
    }
}
