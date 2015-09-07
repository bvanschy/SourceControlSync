using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace SourceControlSync.Domain
{
    public class ChangesReport : ErrorReport, IChangesReport
    {
        public ChangesReport(IClock clock)
            : base(clock)
        {
        }

        public IExecutedCommands ExecutedCommands { get; set; }

        public override bool HasMessage
        {
            get { return ExecutedCommands != null || Exception != null; }
        }

        public override MailMessage ToMailMessage()
        {
            var message = new MailMessage()
            {
                Body = GetMessageBody(),
                IsBodyHtml = false
            };

            message.Attachments.Add(GetRequestAttachment());

            return message;
        }

        private string GetMessageBody()
        {
            var body = new StringBuilder();

            if (ExecutedCommands != null)
            {
                body.AppendLine(GetMessageBodyForSuccess());
            }
            if (Exception != null)
            {
                body.AppendLine(GetMessageBodyForError());
            }
            body.AppendLine();

            body.AppendLine(GetDurationString(Thread.CurrentThread.CurrentUICulture));
            body.AppendLine();

            return body.ToString();
        }

        private string GetMessageBodyForError()
        {
            return Exception.Message;
        }

        private string GetMessageBodyForSuccess()
        {
            return ExecutedCommands.ToString();
        }
    }
}
