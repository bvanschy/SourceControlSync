using SourceControlSync.Domain.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IItemCommand : IDisposable
    {
        Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token);
    }
}
