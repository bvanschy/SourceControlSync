using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Extensions
{
    public static class ItemChangeExtensions
    {
        public static Stream CreateContentStream(this ItemChange change)
        {
            if (change.NewContent == null)
            {
                throw new ApplicationException(string.Format("ItemChange for {0} of type {1} must have content", 
                    change.Item.Path,
                    change.ChangeType.ToString()));
            }

            var content = new MemoryStream();
            if (change.NewContent.ContentType == ItemContentType.RawText)
            {
                WriteRawTextToStream(change, content);
            }
            else if (change.NewContent.ContentType == ItemContentType.Base64Encoded)
            {
                WriteEncodedBinaryToStream(change, content);
            }
            content.Position = 0;
            return content;
        }

        private static void WriteEncodedBinaryToStream(ItemChange change, MemoryStream content)
        {
            var binary = Convert.FromBase64String(change.NewContent.Content);
            content.Write(binary, 0, binary.Length);
        }

        private static void WriteRawTextToStream(ItemChange change, MemoryStream content)
        {
            using (var streamWriter = new StreamWriter(content, change.Item.ContentMetadata.Encoding, 1024, true))
            {
                streamWriter.Write(change.NewContent.Content);
            }
        }
    }
}
