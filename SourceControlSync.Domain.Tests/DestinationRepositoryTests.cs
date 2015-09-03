using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
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
                    Item = new Item() { Path = "/test/test.txt" }
                }
            };
            ItemChange itemChanged = null;
            var fakeCommand = new Fakes.StubIItemCommand()
            {
                IsChangeOperableItemChange = (change) => { return true; },
                ExecuteOnDestinationAsyncItemChangeCancellationToken = (change, token) => 
                {
                    itemChanged = change;
                    return Task.FromResult(0); 
                }
            };
            var repo = new DestinationRepository(fakeCommand);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(itemChanged);
            Assert.AreEqual("test/test.txt", itemChanged.Item.Path);
        }

        [TestMethod]
        public void AddItem()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    Item = new Item() { Path = "/test/test.txt" },
                    NewContent = new ItemContent()
                }
            };
            ItemChange itemChanged = null;
            var fakeCommand = new Fakes.StubIItemCommand()
            {
                IsChangeOperableItemChange = (change) => { return true; },
                ExecuteOnDestinationAsyncItemChangeCancellationToken = (change, token) =>
                {
                    itemChanged = change;
                    return Task.FromResult(0);
                }
            };
            var repo = new DestinationRepository(fakeCommand);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(itemChanged);
            Assert.AreEqual("test/test.txt", itemChanged.Item.Path);
            Assert.AreSame(itemChanges.Single().NewContent, itemChanged.NewContent);
        }

        [TestMethod]
        public void ExecutedCommands()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Delete,
                    Item = new Item()
                    {
                        Path = "/test/test1.txt"
                    }
                },
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Add,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata(),
                        Path = "/test/test2.txt"
                    },
                    NewContent = new ItemContent()
                },
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Edit,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata(),
                        Path = "/test/test3.txt"
                    },
                    NewContent = new ItemContent()
                },
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Rename,
                    Item = new Item()
                    {
                        ContentMetadata = new FileContentMetadata(),
                        Path = "/test/test4.txt"
                    },
                    NewContent = new ItemContent()
                },
                new ItemChange()
                {
                    ChangeType = ItemChangeType.Encoding,
                    Item = new Item()
                    {
                        Path = "/test/test5.txt"
                    }
                }
            };
            var fakeDeleteCommand = new Fakes.StubIItemCommand()
            {
                IsChangeOperableItemChange = (change) => { return change.ChangeType == ItemChangeType.Delete; },
                ExecuteOnDestinationAsyncItemChangeCancellationToken = (itemChange, token) => { return Task.FromResult(0); }
            };
            var fakeUploadCommand = new Fakes.StubIItemCommand()
            {
                IsChangeOperableItemChange = (change) => 
                { 
                    return change.ChangeType == ItemChangeType.Add ||
                        change.ChangeType == ItemChangeType.Edit ||
                        change.ChangeType == ItemChangeType.Rename; 
                },
                ExecuteOnDestinationAsyncItemChangeCancellationToken = (itemChange, token) => { return Task.FromResult(0); }
            };
            var fakeNullCommand = new Fakes.StubIItemCommand()
            {
                IsChangeOperableItemChange = (change) => { return change.ChangeType == ItemChangeType.Encoding; },
                ExecuteOnDestinationAsyncItemChangeCancellationToken = (itemChange, token) => { return Task.FromResult(0); }
            };
            var repo = new DestinationRepository(fakeDeleteCommand, fakeUploadCommand, fakeNullCommand);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(repo.ExecutedCommands);
            Assert.AreEqual(5, repo.ExecutedCommands.Count());
            foreach (var pair in repo.ExecutedCommands)
            {
                switch (pair.ItemChange.Item.Path)
                {
                    case "test/test1.txt":
                        Assert.AreSame(fakeDeleteCommand, pair.ItemCommand);
                        break;
                    case "test/test2.txt":
                        Assert.AreSame(fakeUploadCommand, pair.ItemCommand);
                        break;
                    case "test/test3.txt":
                        Assert.AreSame(fakeUploadCommand, pair.ItemCommand);
                        break;
                    case "test/test4.txt":
                        Assert.AreSame(fakeUploadCommand, pair.ItemCommand);
                        break;
                    case "test/test5.txt":
                        Assert.AreSame(fakeNullCommand, pair.ItemCommand);
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            }
        }
    }
}
