using System;

namespace SourceControlSync.Domain.Models
{
    [Flags]
    public enum ItemChangeType
    {
        None = 0,
        Add = 1,
        Edit = 2,
        Encoding = 4,
        Rename = 8,
        Delete = 16,
        Undelete = 32,
        SourceRename = 1024
    }

    public class ItemChange
    {
        public ItemChangeType ChangeType { get; set; }
        public Item Item { get; set; }
        public ItemContent NewContent { get; set; }
    }
}
