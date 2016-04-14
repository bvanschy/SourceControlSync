using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    /// <summary>
    /// Generic destination repository client requiring a specific data context
    /// </summary>
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
                   select CreateItemChangeInRoot(change);
        }

        /// <summary>
        /// Creates a changed item and removes the root path from the item's full path
        /// </summary>
        private ItemChange CreateItemChangeInRoot(ItemChange change)
        {
            var item = new Item(change.Item.Path.Substring(_root.Length))
            {
                ContentMetadata = change.Item.ContentMetadata,
            };
            return new ItemChange(change.ChangeType, item)
            {
                NewContent = change.NewContent
            };
        }

    }
}
