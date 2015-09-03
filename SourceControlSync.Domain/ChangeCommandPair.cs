using SourceControlSync.Domain.Models;

namespace SourceControlSync.Domain
{
    public class ChangeCommandPair
    {
        public ItemChange ItemChange { get; set; }

        public IItemCommand ItemCommand { get; set; }
    }
}
