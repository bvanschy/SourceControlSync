using Microsoft.VisualStudio.TestTools.UnitTesting;
using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Tests
{
    [TestClass]
    public class DestinationRepositoryTests
    {
        [TestMethod]
        public void PushItemChange()
        {
            var item = new Item("/test/test.txt") 
            {
                ContentMetadata = new FileContentMetadata("text/plain", Encoding.UTF8)
            };
            var itemChange = new ItemChange(ItemChangeType.Add, item)
            {
                NewContent = new ItemContent(ItemContentType.RawText, "Testing")
            };
            var itemChanges = new ItemChange[] { itemChange };
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
            Assert.AreSame(itemChange.Item.ContentMetadata, itemChangesAdded.Single().Item.ContentMetadata);
            Assert.AreSame(itemChange.NewContent, itemChangesAdded.Single().NewContent);
            Assert.IsTrue(saveChangesCalled);
        }
    }
}
