using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class Item
    {
        public FileContentMetadata ContentMetadata { get; set; }
        public string Path { get; set; }

        public bool IsInRoot(string root)
        {
            return Path.StartsWith(root);
        }
    }
}
