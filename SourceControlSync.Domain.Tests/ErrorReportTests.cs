using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ErrorReportTests
    {
        [TestMethod]
        public void NoException()
        {
            var fakeClock = new Fakes.StubIClock();
            using (var errorReport = new ErrorReport(fakeClock))
            {
                Assert.IsFalse(errorReport.HasMessage);
            }
        }

        [TestMethod]
        public void MailMessage()
        {
            var fakeClock = new Fakes.StubIClock();
            using (var errorReport = new ErrorReport(fakeClock))
            {
                errorReport.Request = string.Empty;
                errorReport.Exception = new Exception("Oops!");

                var message = errorReport.ToMailMessage();

                Assert.IsTrue(errorReport.HasMessage);
                Assert.IsNotNull(message);
                Assert.IsTrue(message.Body.Contains("Oops!"));
            }
        }

        [TestMethod]
        public void MailMessageRequestAttachment()
        {
            var fakeClock = new Fakes.StubIClock();
            using (var errorReport = new ErrorReport(fakeClock))
            {
                errorReport.Request = string.Format("{{ Id: \"{0}\", Message: \"message\" }}", Guid.NewGuid().ToString());
                errorReport.Exception = new Exception("Oops!");

                var message = errorReport.ToMailMessage();

                Assert.AreEqual(1, message.Attachments.Count);
                Assert.AreEqual(ErrorReport.ATTACHMENT_CONTENTTYPE, message.Attachments.Single().ContentType.MediaType);
                Assert.IsNotNull(message.Attachments.Single().ContentStream);
            }
        }

        [TestMethod]
        public void MailMessageDuration()
        {
            var times = new DateTime[]
            {
                new DateTime(2015, 9, 2, 3, 0, 0, 0),
                new DateTime(2015, 9, 2, 3, 0, 1, 333)
            };
            int i = 0;
            var fakeClock = new Fakes.StubIClock()
            {
                UtcNowGet = () => { return times[i++]; }
            };
            using (var errorReport = new ErrorReport(fakeClock))
            {
                errorReport.Request = string.Empty;
                errorReport.Exception = new Exception("Oops!");

                var message = errorReport.ToMailMessage();

                Assert.IsTrue(message.Body.Contains("Took 1.33 seconds"));
            }
        }
    }
}
