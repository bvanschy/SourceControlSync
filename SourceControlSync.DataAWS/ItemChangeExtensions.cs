using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.DataAWS
{
    public static class ItemChangeExtensions
    {
        public static IEnumerable<IItemCommand> CreateItemCommands(this IEnumerable<ItemChange> itemChanges)
        {
            return itemChanges.Select(c => c.CreateItemCommand());
        }

        public static IItemCommand CreateItemCommand(this ItemChange itemChange)
        {
            if ((itemChange.ChangeType & ItemChangeType.Delete) != 0)
            {
                return new DeleteItemCommand(itemChange);
            }
            else if (itemChange.NewContent != null)
            {
                return new UploadItemCommand(itemChange);
            }
            else
            {
                return new NullItemCommand();
            }
        }
    }
}
