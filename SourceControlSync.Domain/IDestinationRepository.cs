using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDestinationRepository : IDisposable
    {
        IList<ChangeCommandPair> ExecutedCommands { get; }

        Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root);
    }
}
