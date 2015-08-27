using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Models
{
    public enum ItemContentType
    {
        RawText,
        Base64Encoded
    }

    public class ItemContent
    {
        public static async Task<ItemContent> CreateItemContentFromBinaryStreamAsync(Stream content, CancellationToken token)
        {
            using (var memoryStream = new MemoryStream())
            {
                const int BUFFER_SIZE = 4096;
                await content.CopyToAsync(memoryStream, BUFFER_SIZE, cancellationToken: token);
                var bytes = memoryStream.ToArray();
                return new ItemContent()
                {
                    ContentType = ItemContentType.Base64Encoded,
                    Content = Convert.ToBase64String(bytes)
                };
            }
        }

        public static async Task<ItemContent> CreateItemContentFromTextStreamAsync(Stream content, Encoding encoding)
        {
            using (var streamReader = new StreamReader(content, encoding))
            {
                return new ItemContent()
                {
                    ContentType = ItemContentType.RawText,
                    Content = await streamReader.ReadToEndAsync()
                };
            }
        }

        public ItemContentType ContentType { get; set; }

        public string Content { get; set; }

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
