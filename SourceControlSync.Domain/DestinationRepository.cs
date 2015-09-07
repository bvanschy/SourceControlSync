using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class DestinationRepository : IDestinationRepository
    {
        private readonly IDestinationContext _destinationContext;

        public DestinationRepository(IDestinationContext destinationContext)
        {
            _destinationContext = destinationContext;
        }

        public IExecutedCommands ExecutedCommands { get; private set; }

        public async Task PushItemChangesAsync(IEnumerable<ItemChange> itemChanges, string root)
        {
            ExecutedCommands = null;
            _destinationContext.AddItemChanges(GetItemChangesInRoot(itemChanges, root));
            await _destinationContext.SaveChangesAsync(CancellationToken.None);
            ExecutedCommands = _destinationContext.ExecutedCommands;
        }

        private static IEnumerable<ItemChange> GetItemChangesInRoot(IEnumerable<ItemChange> itemChanges, string root)
        {
            return from change in itemChanges
                   where !string.IsNullOrEmpty(change.Item.Path) && change.Item.IsInRoot(root)
                   select new ItemChange()
                   {
                       ChangeType = change.ChangeType,
                       Item = new Item()
                       {
                           ContentMetadata = change.Item.ContentMetadata,
                           Path = change.Item.Path.Substring(root.Length)
                       },
                       NewContent = change.NewContent
                   };
        }
    }
}
