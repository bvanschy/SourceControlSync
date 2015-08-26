using SourceControlSync.Domain.Models;

namespace SourceControlSync.Domain.Extensions
{
    public static class ItemExtensions
    {
        public static bool IsInRoot(this Item item, string root)
        {
            return item.Path.StartsWith(root);
        }
    }
}
