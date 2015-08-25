using SourceControlSync.Domain.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public class NullItemCommand : IItemCommand
    {
        public Task ExecuteOnDestinationAsync(ItemChange itemChange, CancellationToken token)
        {
            return Task.FromResult(0);
        }

        public void Dispose()
        {
        }
    }
}
