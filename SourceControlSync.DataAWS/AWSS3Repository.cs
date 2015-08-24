using SourceControlSync.Domain;
using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public class AWSS3Repository : IDestinationRepository
    {
        private readonly IItemCommand _deleteCommand;
        private readonly IItemCommand _uploadCommand;
        private readonly IItemCommand _nullCommand;

        public AWSS3Repository(ICommandFactory commandFactory)
        {
            _deleteCommand = commandFactory.CreateDeleteCommand();
            _uploadCommand = commandFactory.CreateUploadCommand(); ;
            _nullCommand = commandFactory.CreateNullCommand();
        }

        public string ConnectionString 
        { 
            set
            {
                _deleteCommand.ConnectionString = value;
                _uploadCommand.ConnectionString = value;
                _nullCommand.ConnectionString = value;
            }
        }

        public async Task PushItemChangesAsync(IEnumerable<ItemChange> itemChanges, string root)
        {
            var tasks = from itemChange in GetItemChangesInRoot(itemChanges, root)
                        select ExecuteItemChangeOnS3BucketAsync(itemChange);
            await Task.WhenAll(tasks);
        }

        private static IEnumerable<ItemChange> GetItemChangesInRoot(IEnumerable<ItemChange> itemChanges, string root)
        {
            return from change in itemChanges
                   where !string.IsNullOrEmpty(change.Item.Path) && change.Item.Path.StartsWith(root)
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

        private Task ExecuteItemChangeOnS3BucketAsync(ItemChange itemChange)
        {
            var command = GetItemChangeCommand(itemChange);
            return command.ExecuteOnS3BucketAsync(itemChange, CancellationToken.None);
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
    }
}
