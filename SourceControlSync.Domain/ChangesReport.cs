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

        public IList<ChangeCommandPair> ExecutedCommands { get; set; }

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
                GetMessageBodyForSuccess(body);
            }

            if (Exception != null)
            {
                GetMessageBodyForError(body);
            }

            body.AppendLine();
            body.AppendLine(GetDurationString(Thread.CurrentThread.CurrentUICulture));
            body.AppendLine();

            return body.ToString();
        }

        private void GetMessageBodyForError(StringBuilder body)
        {
            body.AppendLine(Exception.Message);
        }

        private void GetMessageBodyForSuccess(StringBuilder body)
        {
            var executedCommandsLookup = ExecutedCommands.ToLookup(kv => kv.ItemChange.Item.Path, kv => kv.ItemCommand.ToString());
            foreach (var commands in executedCommandsLookup.OrderBy(kv => kv.Key))
            {
                body.Append(commands.Key)
                    .Append("\t")
                    .AppendLine(string.Join(",", commands));
            }
        }
    }
}
