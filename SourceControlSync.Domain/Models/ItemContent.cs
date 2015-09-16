using System;
using System.IO;
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
        public ItemContent(ItemContentType contentType, string content)
        {
            ContentType = contentType;
            Content = content;
        }

        public ItemContentType ContentType { get; private set; }

        public string Content { get; private set; }

        public void WriteToStream(Stream contentStream, Encoding encoding)
        {
            if (ContentType == ItemContentType.RawText)
            {
                WriteRawTextToStream(contentStream, encoding);
            }
            else if (ContentType == ItemContentType.Base64Encoded)
            {
                WriteEncodedBinaryToStream(contentStream);
            }
        }

        private void WriteEncodedBinaryToStream(Stream contentStream)
        {
            var binary = Convert.FromBase64String(Content);
            contentStream.Write(binary, 0, binary.Length);
        }

        private void WriteRawTextToStream(Stream contentStream, Encoding encoding)
        {
            const int BUFFER_SIZE = 1024;
            using (var streamWriter = new StreamWriter(contentStream, encoding, BUFFER_SIZE, true))
            {
                streamWriter.Write(Content);
            }
        }
    }
}
