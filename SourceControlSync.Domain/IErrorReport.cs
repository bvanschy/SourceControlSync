using System;
using System.Net.Mail;

namespace SourceControlSync.Domain
{
    public interface IErrorReport : IDisposable
    {
        string Request { get; set; }

        Exception Exception { get; set; }

        bool HasMessage { get; }

        MailMessage ToMailMessage();
    }
}
