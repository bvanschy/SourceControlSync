using Newtonsoft.Json;
using SourceControlSync.DataAWS;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace SourceControlSync.WebApi.Controllers
{
    public class VSOController : ApiController
    {
        public const string HEADER_VSO_BASE_URL = "VSO-BaseUrl";
        public const string HEADER_VSO_USER_NAME = "VSO-UserName";
        public const string HEADER_VSO_ACCESS_TOKEN = "VSO-AccessToken";
        public const string HEADER_VSO_ROOT = "VSO-Root";
        public const string HEADER_AWS_ACCESS_KEY_ID = "AWS-AccessKeyId";
        public const string HEADER_AWS_SECRET_ACCESS_KEY = "AWS-SecretAccessKey";
        public const string HEADER_AWS_REGION_SYSTEM_NAME = "AWS-RegionSystemName";
        public const string HEADER_AWS_BUCKET_NAME = "AWS-BucketName";

        private ISourceRepository _sourceRepository;
        private IChangesCalculator _changesCalculator;
        private IDestinationRepository _destinationRepository;

        public VSOController()
        {
        }

        public VSOController(ISourceRepository vsoRepository, IChangesCalculator changesCalculator, IDestinationRepository destRepository)
        {
            _sourceRepository = vsoRepository;
            _changesCalculator = changesCalculator;
            _destinationRepository = destRepository;
        }

        public async Task<IHttpActionResult> Post(VSOCodePushed data)
        {
            var vsoBaseUrl = GetHeaderValue(HEADER_VSO_BASE_URL);
            var vsoUserName = GetHeaderValue(HEADER_VSO_USER_NAME);
            var vsoAccessToken = GetHeaderValue(HEADER_VSO_ACCESS_TOKEN);
            var vsoRoot = GetHeaderValue(HEADER_VSO_ROOT);

            var awsAccessKeyId = GetHeaderValue(HEADER_AWS_ACCESS_KEY_ID);
            var awsSecretAccessKey = GetHeaderValue(HEADER_AWS_SECRET_ACCESS_KEY);
            var awsRegion = GetHeaderValue(HEADER_AWS_REGION_SYSTEM_NAME);
            var awsBucketName = GetHeaderValue(HEADER_AWS_BUCKET_NAME);

            string content = JsonConvert.SerializeObject(data, Formatting.Indented);
            Trace.TraceInformation("Visual Studio Online posted an event from {0}:{1}:{2} to {3}:{4}{5}{6}{5}", 
                vsoUserName, vsoBaseUrl, vsoRoot,
                awsRegion, awsBucketName,
                Environment.NewLine, content);

            try
            {
                if (vsoBaseUrl != null && vsoUserName != null && vsoAccessToken != null && vsoRoot != null &&
                    awsAccessKeyId != null && awsSecretAccessKey != null && awsRegion != null && awsBucketName != null)
                {
                    if (_sourceRepository == null)
                    {
                        _sourceRepository = new VSORepository(vsoBaseUrl, vsoUserName, vsoAccessToken);
                    }
                    if (_changesCalculator == null)
                    {
                        _changesCalculator = new ChangesCalculator();
                    }
                    if (_destinationRepository == null)
                    {
                        _destinationRepository = new AWSS3Repository(awsAccessKeyId, awsSecretAccessKey, awsRegion, awsBucketName);
                    }

                    var push = data.ToSync();
                    await _sourceRepository.DownloadChangesAsync(push, vsoRoot);
                    var itemChanges = _changesCalculator.CalculateItemChanges(push.Commits);
                    await _destinationRepository.PushItemChangesAsync(itemChanges, vsoRoot);

                    return Ok();
                }
                else
                {
                    return BadRequest("Missing headers");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                Trace.TraceError(e.StackTrace);
                return InternalServerError(e);
            }
            finally
            {
                Trace.Flush();
            }
        }

        private string GetHeaderValue(string headerName)
        {
            IEnumerable<string> values;
            if (Request.Headers.TryGetValues(headerName, out values))
            {
                return values.FirstOrDefault();
            }
            return null;
        }
    }
}
