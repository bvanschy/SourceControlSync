using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class DestinationRepositoryTests
    {
        [TestMethod]
        public void DeleteItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Delete,
                    Item = new Item()
                    {
                        Path = "/test/test.txt"
                    }
                }
            };
            var fakeCommand = new FakeCommand();
            var repo = new DestinationRepository(fakeCommand, null, null);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(fakeCommand.ItemChangeExecuted);
            Assert.AreEqual("test/test.txt", fakeCommand.ItemChangeExecuted.Item.Path);
        }

        [TestMethod]
        public void AddItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Add,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata()
                        {
                            ContentType = "text/plain",
                            Encoding = Encoding.UTF8
                        },
                        Path = "/test/test.txt"
                    },
                    NewContent = new ItemContent()
                    {
                        ContentType = ItemContentType.RawText,
                        Content = "This is a test"
                    }
                }
            };
            var fakeCommand = new FakeCommand();
            var repo = new DestinationRepository(null, fakeCommand, null);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(fakeCommand.ItemChangeExecuted);
            Assert.AreEqual("test/test.txt", fakeCommand.ItemChangeExecuted.Item.Path);
            Assert.AreSame(itemChanges.Single().NewContent, fakeCommand.ItemChangeExecuted.NewContent);
        }

        [TestMethod]
        public void EditItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Edit,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata()
                        {
                            ContentType = "text/plain",
                            Encoding = Encoding.UTF8
                        },
                        Path = "/test/test.txt"
                    },
                    NewContent = new ItemContent()
                    {
                        ContentType = ItemContentType.RawText,
                        Content = "This is a test"
                    }
                }
            };
            var fakeCommand = new FakeCommand();
            var repo = new DestinationRepository(null, fakeCommand, null);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(fakeCommand.ItemChangeExecuted);
            Assert.AreEqual("test/test.txt", fakeCommand.ItemChangeExecuted.Item.Path);
            Assert.AreSame(itemChanges.Single().NewContent, fakeCommand.ItemChangeExecuted.NewContent);
        }

        [TestMethod]
        public void RenameItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Rename,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata()
                        {
                            ContentType = "text/plain",
                            Encoding = Encoding.UTF8
                        },
                        Path = "/test/test.txt"
                    },
                    NewContent = new ItemContent()
                    {
                        ContentType = ItemContentType.RawText,
                        Content = "This is a test"
                    }
                }
            };
            var fakeCommand = new FakeCommand();
            var repo = new DestinationRepository(null, fakeCommand, null);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(fakeCommand.ItemChangeExecuted);
            Assert.AreEqual("test/test.txt", fakeCommand.ItemChangeExecuted.Item.Path);
            Assert.AreSame(itemChanges.Single().NewContent, fakeCommand.ItemChangeExecuted.NewContent);
        }

        [TestMethod]
        public void EncodeItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Encoding,
                    Item = new Item()
                    {
                        Path = "/test/test.txt"
                    }
                }
            };
            var fakeCommand = new FakeCommand();
            var repo = new DestinationRepository(null, null, fakeCommand);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(fakeCommand.ItemChangeExecuted);
            Assert.AreEqual("test/test.txt", fakeCommand.ItemChangeExecuted.Item.Path);
        }

        private class FakeCommand : IItemCommand
        {
            public ItemChange ItemChangeExecuted { get; set; }

            public string ConnectionString { get; set; }

            public Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token)
            {
                ItemChangeExecuted = itemChange;
                return Task.FromResult(0);
            }

            public void Dispose()
            {
            }
        }
    }
}
