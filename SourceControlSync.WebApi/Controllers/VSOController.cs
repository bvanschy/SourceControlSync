using Newtonsoft.Json;
using SourceControlSync.DataAWS;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using SourceControlSync.WebApi.Models;
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

        private VSOCodePushed _pushEvent;
        private CancellationToken _token;
        private HeaderParameters _parameters;

        private ISourceRepository _sourceRepository;
        private IChangesCalculator _changesCalculator;
        private IDestinationRepository _destinationRepository;

        public ISourceRepository SourceRepository 
        {
            get 
            { 
                _sourceRepository = _sourceRepository ?? new VSORepository();
                return _sourceRepository;
            }
            set { _sourceRepository = value; }
        }
        public IChangesCalculator ChangesCalculator 
        {
            get 
            {
                _changesCalculator = _changesCalculator ?? new ChangesCalculator();
                return _changesCalculator; 
            }
            set { _changesCalculator = value; }
        }
        public IDestinationRepository DestinationRepository 
        {
            get 
            { 
                _destinationRepository = _destinationRepository ?? new AWSS3Repository(new AWSS3CommandFactory());
                return _destinationRepository;
            }
            set { _destinationRepository = value; }
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
            string content = JsonConvert.SerializeObject(_pushEvent, Formatting.Indented);
            Trace.TraceInformation("Visual Studio Online posted an event {0}{1}{0}", Environment.NewLine, content);
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
            SourceRepository.ConnectionString = _parameters[HEADER_SOURCE_CONNECTIONSTRING];
            DestinationRepository.ConnectionString = _parameters[HEADER_DESTINATION_CONNECTIONSTRING];

            await SourceRepository.DownloadChangesAsync(push, root, _token);
            ChangesCalculator.CalculateItemChanges(push.Commits);
            await DestinationRepository.PushItemChangesAsync(ChangesCalculator.ItemChanges, root);
        }
    }
}
