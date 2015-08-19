using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class FileContentMetadata
    {
        public bool IsBinary { get; set; }

        public string ContentType { get; set; }

        public Encoding Encoding { get; set; }
    }
}
