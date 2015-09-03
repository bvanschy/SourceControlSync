using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace SourceControlSync.Domain
{
    public class ErrorReport : IErrorReport
    {
        public const string ATTACHMENT_NAME = "request.txt";
        public const string ATTACHMENT_CONTENTTYPE = "text/plain";

        private readonly IClock _clock;
        private readonly DateTime _startTime;

        private Stream _attachmentStream;

        public ErrorReport(IClock clock)
        {
            _clock = clock;
            _startTime = _clock.UtcNow;
        }

        public string Request { get; set; }

        public Exception Exception { get; set; }

        public virtual bool HasMessage 
        {
            get { return Exception != null; }
        }

        public virtual MailMessage ToMailMessage()
        {
            var message = new MailMessage()
            {
                Body = GetMessageBody(),
                IsBodyHtml = false
            };

            message.Attachments.Add(GetRequestAttachment());

            return message;
        }

        protected Attachment GetRequestAttachment()
        {
            var attachmentStream = CreateRequestStream();
            var attachment = new Attachment(attachmentStream, ATTACHMENT_NAME, ATTACHMENT_CONTENTTYPE);
            return attachment;
        }

        private Stream CreateRequestStream()
        {
            var requestBytes = Encoding.UTF8.GetBytes(Request);
            ClearAttachment();
            _attachmentStream = new MemoryStream(requestBytes);
            return _attachmentStream;
        }

        protected string GetDurationString(CultureInfo culture)
        {
            var duration = _clock.UtcNow - _startTime;
            var durationText = Resources.ResourceManager.GetString("ReportDurationText", culture);
            return string.Format(culture, durationText, duration.TotalSeconds);
        }

        private string GetMessageBody()
        {
            var body = new StringBuilder();
            body.AppendLine(GetDurationString(CultureInfo.InvariantCulture))
                .AppendLine()
                .AppendLine(Exception.ToString());
            return body.ToString();
        }

        public virtual void Dispose()
        {
            ClearAttachment();
        }

        private void ClearAttachment()
        {
            if (_attachmentStream != null)
            {
                _attachmentStream.Dispose();
                _attachmentStream = null;
            }
        }
    }
}
