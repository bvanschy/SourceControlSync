using Newtonsoft.Json;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using SourceControlSync.WebApi.Models;
using SourceControlSync.WebApi.Util;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SourceControlSync.WebApi.Controllers
{
    public class VSOController : ApiController
    {
        public const string HEADER_ROOT = "Sync-Root";
        public const string HEADER_SOURCE_CONNECTIONSTRING = "Sync-SourceConnectionString";
        public const string HEADER_DESTINATION_CONNECTIONSTRING = "Sync-DestConnectionString";

        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IChangesCalculator _changesCalculator;
        private readonly ILogger _logger;

        private VSOCodePushed _pushEvent;
        private CancellationToken _token;
        private HeaderParameters _parameters;

        public VSOController(IRepositoryFactory repositoryFactory, IChangesCalculator changesCalculator, ILogger logger)
        {
            _repositoryFactory = repositoryFactory;
            _changesCalculator = changesCalculator;
            _logger = logger;
        }

        public async Task<IHttpActionResult> PostAsync(VSOCodePushed data, CancellationToken token)
        {
            _pushEvent = data;
            _token = token;
            _parameters = new HeaderParameters(Request.Headers,
                HEADER_ROOT,
                HEADER_SOURCE_CONNECTIONSTRING,
                HEADER_DESTINATION_CONNECTIONSTRING);

            if (!_parameters.AnyMissing)
            {
                LogRequest();
                var result = await HandleSynchronizePushAsync();
                return result;
            }
            else
            {
                return BadRequest("Missing headers");
            }
        }

        private void LogRequest()
        {
            if (_logger != null)
            {
                string content = JsonConvert.SerializeObject(_pushEvent, Formatting.Indented);
                _logger.TraceInformation("Visual Studio Online posted an event {0}{1}{0}", Environment.NewLine, content);
            }
        }

        private async Task<IHttpActionResult> HandleSynchronizePushAsync()
        {
            try
            {
                await SynchronizePushAsync();
                return Ok();
            }
            catch (AggregateException e)
            {
                Trace.TraceError(e.InnerException.ToString());
                return InternalServerError(e.InnerException);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                return InternalServerError(e);
            }
            finally
            {
                Trace.Flush();
            }
        }

        private async Task SynchronizePushAsync()
        {
            var push = _pushEvent.ToSync();
            string root = _parameters[HEADER_ROOT];
            using (var sourceRepository = _repositoryFactory.CreateSourceRepository(_parameters[HEADER_SOURCE_CONNECTIONSTRING]))
            using (var destinationRepository = _repositoryFactory.CreateDestinationRepository(_parameters[HEADER_DESTINATION_CONNECTIONSTRING]))
            {
                await sourceRepository.DownloadChangesAsync(push, root, _token);
                _changesCalculator.CalculateItemChanges(push.Commits);
                await destinationRepository.PushItemChangesAsync(_changesCalculator.ItemChanges, root);
            }
        }
    }
}
