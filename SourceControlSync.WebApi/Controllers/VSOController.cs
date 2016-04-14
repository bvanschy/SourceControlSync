using Microsoft.Azure;
using Newtonsoft.Json;
using SourceControlSync.DataVSO;
using SourceControlSync.Domain;
using SourceControlSync.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SourceControlSync.WebApi.Controllers
{
    /// <summary>
    /// Handles Git push messages from Visual Studio Team Services
    /// </summary>
    public class VSOController : ApiController
    {
        public const string HEADER_ROOT = "Sync-Root";
        public const string HEADER_SOURCE_CONNECTIONSTRING = "Sync-SourceConnectionString";
        public const string HEADER_DESTINATION_CONNECTIONSTRING = "Sync-DestConnectionString";

        private readonly ISourceRepositoryFactory _sourceRepositoryFactory;
        private readonly IDestinationRepositoryFactory _destinationRepositoryFactory;
        private readonly IChangesCalculator _changesCalculator;
        private readonly IChangesReport _changesReport;
        private readonly IErrorReport _errorReport;

        private VSOCodePushed _pushEvent;
        private CancellationToken _token;
        private HeaderParameters _parameters;

        public VSOController(
            ISourceRepositoryFactory sourceRepositoryFactory,
            IDestinationRepositoryFactory destinationRepositoryFactory,
            IChangesCalculator changesCalculator, 
            IChangesReport changesReport, 
            IErrorReport errorReport
            )
        {
            _sourceRepositoryFactory = sourceRepositoryFactory;
            _destinationRepositoryFactory = destinationRepositoryFactory;
            _changesCalculator = changesCalculator;
            _changesReport = changesReport;
            _errorReport = errorReport;
        }

        /// <summary>
        /// Processes a push message by copying files from Git to a destination repository.
        /// Reports any errors and successfully copied files.
        /// </summary>
        /// <param name="data">The deserialized push message</param>
        /// <returns></returns>
        public async Task<IHttpActionResult> PostAsync(VSOCodePushed data, CancellationToken token)
        {
            _parameters = new HeaderParameters(Request.Headers,
                HEADER_ROOT,
                HEADER_SOURCE_CONNECTIONSTRING,
                HEADER_DESTINATION_CONNECTIONSTRING);

            if (!_parameters.AnyMissing)
            {
                var request = JsonConvert.SerializeObject(data, Formatting.Indented);
                _errorReport.Request = request;
                _changesReport.Request = request;

                _pushEvent = data;
                _token = token;

                var result = await HandleSynchronizePushAsync();
                await SendExceptionReportAsync();
                await SendChangesReportAsync();
                return result;
            }
            else
            {
                return BadRequest("Missing headers");
            }
        }

        private async Task<IHttpActionResult> HandleSynchronizePushAsync()
        {
            try
            {
                var executedCommands = await SynchronizePushAsync();
                _changesReport.ExecutedCommands = executedCommands;
                return Ok();
            }
            catch (AggregateException e)
            {
                _errorReport.Exception = e.InnerException;
                _changesReport.Exception = e.InnerException;
                return InternalServerError(e.InnerException);
            }
            catch (Exception e)
            {
                _errorReport.Exception = e;
                _changesReport.Exception = e;
                return InternalServerError(e);
            }
        }

        private async Task<IExecutedCommands> SynchronizePushAsync()
        {
            var push = _pushEvent.ToSync();
            string root = _parameters[HEADER_ROOT];

            // Create the source repository client and perform the downloads
            var sourceConnectionString = _parameters[HEADER_SOURCE_CONNECTIONSTRING];
            var sourceRepository = _sourceRepositoryFactory.CreateSourceRepository(sourceConnectionString);
            await sourceRepository.DownloadChangesAsync(push, root, _token);

            _changesCalculator.CalculateItemChanges(push.Commits);

            // Create the destination repository client and perform the uploads
            var destinationConnectionString = _parameters[HEADER_DESTINATION_CONNECTIONSTRING];
            var destinationRepository = _destinationRepositoryFactory.CreateDestinationRepository(destinationConnectionString);
            await destinationRepository.PushItemChangesAsync(_changesCalculator.ItemChanges, root);

            return destinationRepository.ExecutedCommands;
        }

        /// <summary>
        /// Email a report of all the successful changes to the owner(s) of the Git repository
        /// </summary>
        private async Task SendChangesReportAsync()
        {
            if (!_changesReport.HasMessage)
            {
                return;
            }

            var recipients = _pushEvent.Resource.Commits.GetCommitterEmails();
            if (recipients.Any())
            {
                var mailMessage = _changesReport.ToMailMessage();

                var fromEmailAddress = CloudConfigurationManager.GetSetting("ChangesReportFromEmailAddress");
                mailMessage.From = new MailAddress(fromEmailAddress);
                var subject = CloudConfigurationManager.GetSetting("ChangesReportSubject");
                mailMessage.Subject = subject;
                mailMessage.To.Add(string.Join(",", recipients));

                await HandleSendMailAsync(mailMessage);
            }
        }

        /// <summary>
        /// Email an error report to the system administrator
        /// </summary>
        private async Task SendExceptionReportAsync()
        {
            if (!_errorReport.HasMessage)
            {
                return;
            }

            var mailMessage = _errorReport.ToMailMessage();

            var fromEmailAddress = CloudConfigurationManager.GetSetting("ErrorReportFromEmailAddress");
            mailMessage.From = new MailAddress(fromEmailAddress);
            var subject = CloudConfigurationManager.GetSetting("ErrorReportSubject");
            mailMessage.Subject = subject;
            var recipient = CloudConfigurationManager.GetSetting("ErrorReportToEmailAddress");
            mailMessage.To.Add(recipient);

            await HandleSendMailAsync(mailMessage);
        }

        private static async Task HandleSendMailAsync(MailMessage mailMessage)
        {
            try
            {
                await SendMailAsync(mailMessage);
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        private static async Task SendMailAsync(MailMessage mailMessage)
        {
            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_errorReport != null)
                {
                    _errorReport.Dispose();
                }
                if (_changesReport != null)
                {
                    _changesReport.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
