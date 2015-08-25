using SourceControlSync.Domain.Extensions;
using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class DestinationRepository : IDestinationRepository
    {
        private readonly IItemCommand _deleteCommand;
        private readonly IItemCommand _uploadCommand;
        private readonly IItemCommand _nullCommand;

        public DestinationRepository(IItemCommand deleteCommand, IItemCommand uploadCommand, IItemCommand nullCommand)
        {
            _deleteCommand = deleteCommand;
            _uploadCommand = uploadCommand;
            _nullCommand = nullCommand;
        }

        public async Task PushItemChangesAsync(IEnumerable<ItemChange> itemChanges, string root)
        {
            var tasks = from itemChange in GetItemChangesInRoot(itemChanges, root)
                        select ExecuteItemChangeOnDestinationAsync(itemChange);
            await Task.WhenAll(tasks);
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

        private Task ExecuteItemChangeOnDestinationAsync(ItemChange itemChange)
        {
            var command = GetItemChangeCommand(itemChange);
            return command.ExecuteOnDestinationAsync(itemChange, CancellationToken.None);
        }

        private IItemCommand GetItemChangeCommand(ItemChange itemChange)
        {
            if (IsDeleteOperation(itemChange))
            {
                return _deleteCommand;
            }
            else if (IsUploadOperation(itemChange))
            {
                return _uploadCommand;
            }
            else
            {
                return _nullCommand;
            }
        }

        private static bool IsDeleteOperation(ItemChange itemChange)
        {
            return (itemChange.ChangeType & ItemChangeType.Delete) != 0;
        }

        private static bool IsUploadOperation(ItemChange itemChange)
        {
            return itemChange.NewContent != null;
        }

        public void Dispose()
        {
            if (_deleteCommand != null)
            {
                _deleteCommand.Dispose();
            }
            if (_uploadCommand != null)
            {
                _uploadCommand.Dispose();
            }
            if (_nullCommand != null)
            {
                _nullCommand.Dispose();
            }
        }
    }
}
