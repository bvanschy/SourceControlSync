﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class ChangesReportTests
    {
        [TestMethod]
        public void NoCommandsOrException()
        {
            var fakeClock = new Fakes.StubIClock();
            using (var changesReport = new ChangesReport(fakeClock))
            {
                Assert.IsFalse(changesReport.HasMessage);
            }
        }

        [TestMethod]
        public void MailMessageWithNoException()
        {
            var fakeClock = new Fakes.StubIClock();
            var fakeExecutedCommands = new Fakes.StubIExecutedCommands()
            {
                ToString = () => { return "ExecutedCommands"; }
            };
            using (var changesReport = new ChangesReport(fakeClock))
            {
                changesReport.Request = string.Format("{{ Id: \"{0}\", Message: \"message\" }}", Guid.NewGuid().ToString());
                changesReport.ExecutedCommands = fakeExecutedCommands;

                var message = changesReport.ToMailMessage();

                Assert.IsTrue(changesReport.HasMessage);
                Assert.IsFalse(message.IsBodyHtml);
                Assert.IsTrue(message.Body.Contains("ExecutedCommands"));
                Assert.IsTrue(message.Body.Contains("0"));
                Assert.AreEqual(1, message.Attachments.Count);
                Assert.AreEqual(ErrorReport.ATTACHMENT_CONTENTTYPE, message.Attachments.Single().ContentType.MediaType);
                Assert.IsNotNull(message.Attachments.Single().ContentStream);
            }
        }

        [TestMethod]
        public void MailMessageWithException()
        {
            var fakeClock = new Fakes.StubIClock();
            using (var changesReport = new ChangesReport(fakeClock))
            {
                changesReport.Request = string.Empty;
                changesReport.Exception = new Exception("Oops!");

                var message = changesReport.ToMailMessage();

                Assert.IsTrue(changesReport.HasMessage);
                Assert.IsFalse(message.IsBodyHtml);
                Assert.IsTrue(message.Body.Contains("Oops!"));
                Assert.AreEqual(1, message.Attachments.Count);
                Assert.AreEqual(ErrorReport.ATTACHMENT_CONTENTTYPE, message.Attachments.Single().ContentType.MediaType);
                Assert.IsNotNull(message.Attachments.Single().ContentStream);
            }
        }
    }
}
