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

        private IEnumerable<ItemChange> _itemChanges;
        private string _root;

        public DestinationRepository(IDestinationContext destinationContext)
        {
            _destinationContext = destinationContext;
        }

        public IExecutedCommands ExecutedCommands { get; private set; }

        public async Task PushItemChangesAsync(IEnumerable<ItemChange> itemChanges, string root)
        {
            _itemChanges = itemChanges;
            _root = root;

            ExecutedCommands = null;
            _destinationContext.AddItemChanges(GetItemChangesInRoot());
            await _destinationContext.SaveChangesAsync(CancellationToken.None);
            ExecutedCommands = _destinationContext.ExecutedCommands;
        }

        private IEnumerable<ItemChange> GetItemChangesInRoot()
        {
            return from change in _itemChanges
                   where !string.IsNullOrEmpty(change.Item.Path) && change.Item.IsInRoot(_root)
                   select new ItemChange()
                   {
                       ChangeType = change.ChangeType,
                       Item = new Item()
                       {
                           ContentMetadata = change.Item.ContentMetadata,
                           Path = change.Item.Path.Substring(_root.Length)
                       },
                       NewContent = change.NewContent
                   };
        }
    }
}
