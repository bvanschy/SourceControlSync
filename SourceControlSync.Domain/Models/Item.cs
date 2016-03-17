using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class Item
    {
        private bool _isFolder;
        private FileContentMetadata _contentMetadata;

        public Item(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }

        public bool IsFolder
        {
            get { return _isFolder; }
            set { _isFolder = value; }
        }

        public FileContentMetadata ContentMetadata 
        {
            get { return _contentMetadata; }
            set
            {
                if (_contentMetadata != null)
                {
                    throw new InvalidOperationException("ContentMetadata is immutable");
                }
                _contentMetadata = value;
            }
        }

        public bool IsInRoot(string root)
        {
            return Path.StartsWith(root);
        }
    }
}
