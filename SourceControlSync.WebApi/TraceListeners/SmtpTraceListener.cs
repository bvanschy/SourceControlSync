using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SourceControlSync.WebApi.TraceListeners
{
    public class SmtpTraceListener : TraceListener
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _ssl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _from;
        private readonly string _to;
        private readonly string _subject;

        private StringBuilder _message = new StringBuilder();

        public SmtpTraceListener()
        {
        }

        public SmtpTraceListener(string initializeData)
        {
            if (!string.IsNullOrWhiteSpace(initializeData))
            {
                var parms = initializeData.Split(';');
                if (parms.Length == 8)
                {
                    _host = parms[0];
                    _port = int.Parse(parms[1]);
                    _ssl = bool.Parse(parms[2]);
                    _username = parms[3];
                    _password = parms[4];
                    _from = parms[5];
                    _to = parms[6];
                    _subject = parms[7];
                }
            }
        }

        public override void Write(string message)
        {
            _message.Append(message);
        }

        public override void WriteLine(string message)
        {
            _message.AppendLine(message);
        }

        public override void Flush()
        {
            SendEmail(_message.ToString());
            _message.Clear();
            base.Flush();
        }

        private void SendEmail(string message)
        {
            if (!string.IsNullOrWhiteSpace(_host))
            {
                using (var smtpClient = new SmtpClient(_host, _port))
                {
                    smtpClient.EnableSsl = _ssl;
                    smtpClient.Credentials = new NetworkCredential(_username, _password);
                    smtpClient.Send(_from, _to, _subject, message);
                }
            }
        }
    }
}