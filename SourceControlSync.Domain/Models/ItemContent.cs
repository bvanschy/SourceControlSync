using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SourceControlSync.Domain.Models
{
    public enum ItemContentType
    {
        RawText,
        Base64Encoded
    }

    public class ItemContent
    {
        public ItemContentType ContentType { get; set; }

        public string Content { get; set; }
    }
}
