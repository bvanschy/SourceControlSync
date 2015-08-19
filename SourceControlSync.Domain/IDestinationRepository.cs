using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IDestinationRepository
    {
        Task PushItemChangesAsync(IEnumerable<ItemChange> changes, string root);
    }
}
