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
        public void PushItemChange()
        {
            var itemChanges = new List<ItemChange>()
            {
                new ItemChange()
                {
                    Item = new Item() 
                    {
                        ContentMetadata = new FileContentMetadata(),
                        Path = "/test/test.txt" 
                    },
                    NewContent = new ItemContent()
                }
            };
            IEnumerable<ItemChange> itemChangesAdded = null;
            bool saveChangesCalled = false;
            var fakeContext = new Fakes.StubIDestinationContext()
            {
                AddItemChangesIEnumerableOfItemChange = (changes) => { itemChangesAdded = changes; },
                SaveChangesAsyncCancellationToken = (token) => 
                {
                    saveChangesCalled = true;
                    return Task.FromResult(0); 
                }
            };
            var repo = new DestinationRepository(fakeContext);

            repo.PushItemChangesAsync(itemChanges, "/").Wait();

            Assert.IsNotNull(itemChangesAdded);
            Assert.AreEqual("test/test.txt", itemChangesAdded.Single().Item.Path);
            Assert.AreSame(itemChanges.Single().Item.ContentMetadata, itemChangesAdded.Single().Item.ContentMetadata);
            Assert.AreSame(itemChanges.Single().NewContent, itemChangesAdded.Single().NewContent);
            Assert.IsTrue(saveChangesCalled);
        }
    }
}
