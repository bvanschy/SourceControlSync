using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDestinationRepository
    {
        IExecutedCommands ExecutedCommands { get; }

        Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root);
    }
}
