using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class DestinationRepository : IDestinationRepository
    {
        private readonly IList<IItemCommand> _commands;
        private readonly IList<ChangeCommandPair> _executedCommands;

        public DestinationRepository(params IItemCommand[] commands)
            : this(commands.AsEnumerable())
        {
        }

        public DestinationRepository(IEnumerable<IItemCommand> commands)
        {
            _commands = commands.ToList();
            _executedCommands = new List<ChangeCommandPair>();
        }

        public IList<ChangeCommandPair> ExecutedCommands
        {
            get { return _executedCommands; }
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

        private async Task ExecuteItemChangeOnDestinationAsync(ItemChange itemChange)
        {
            foreach (var command in _commands.Where(c => c.IsChangeOperable(itemChange)))
            {
                await command.ExecuteOnDestinationAsync(itemChange, CancellationToken.None);
                var executedCommand = new ChangeCommandPair()
                {
                    ItemChange = itemChange,
                    ItemCommand = command
                };
                _executedCommands.Add(executedCommand);
            }
        }

        public void Dispose()
        {
            foreach (var command in _commands)
            {
                command.Dispose();
            }
            _commands.Clear();
        }
    }
}
