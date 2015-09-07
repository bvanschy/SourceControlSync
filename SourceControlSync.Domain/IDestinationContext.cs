using SourceControlSync.Domain.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDestinationContext
    {
        IExecutedCommands ExecutedCommands { get; }

        void AddItemChanges(IEnumerable<ItemChange> itemChanges);

        Task SaveChangesAsync(CancellationToken token);
    }
}
