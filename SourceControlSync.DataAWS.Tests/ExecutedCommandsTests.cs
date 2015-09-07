using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace SourceControlSync.DataAWS.Tests
{
    [TestClass]
    public class ExecutedCommandsTests
    {
        private CultureInfo _culture;
        private CultureInfo _uiCulture;

        [TestMethod]
        public void Summary()
        {
            var commands = new List<IItemCommand>()
            {
                new DeleteItemCommand(new ItemChange() { Item = new Item() { Path = "index2.html" } }),
                new UploadItemCommand(new ItemChange() { Item = new Item() { Path = "index.html" } }),
                new NullItemCommand()
            };
            var executedCommands = new ExecutedCommands(commands);

            var summary = executedCommands.ToString();

            var expectedSummary = "index.html\tUploaded" + Environment.NewLine + 
                "index2.html\tDeleted" + Environment.NewLine;
            Assert.AreEqual(expectedSummary, summary);
        }

        [TestMethod]
        public void SummaryInFrench()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-CA");
            var commands = new List<IItemCommand>()
            {
                new DeleteItemCommand(new ItemChange() { Item = new Item() { Path = "index2.html" } }),
                new UploadItemCommand(new ItemChange() { Item = new Item() { Path = "index.html" } }),
                new NullItemCommand()
            };
            var executedCommands = new ExecutedCommands(commands);

            var summary = executedCommands.ToString();

            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.StartsWith("index.html"));
            Assert.AreEqual(45, summary.Length);
        }

        [TestInitialize]
        public void InitializeTest()
        {
            _culture = Thread.CurrentThread.CurrentCulture;
            _uiCulture = Thread.CurrentThread.CurrentUICulture;
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Thread.CurrentThread.CurrentCulture = _culture;
            Thread.CurrentThread.CurrentUICulture = _uiCulture;
        }
    }
}
