using SourceControlSync.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            const int BUFFER_SIZE = 1024;
            using (var streamWriter = new StreamWriter(content, change.Item.ContentMetadata.Encoding, BUFFER_SIZE, true))
            {
                streamWriter.Write(change.NewContent.Content);
            }
        }

        public static async Task<ItemContent> CreateItemContentAsync(this ItemChange change, Stream content, CancellationToken token)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (change.Item.ContentMetadata.IsBinary)
            {
                return await CreateEncodedBinaryItemContentAsync(content, token);
            }
            else
            {
                return await CreateRawTextItemContentAsync(change.Item.ContentMetadata.Encoding, content);
            }
        }

        private static async Task<ItemContent> CreateEncodedBinaryItemContentAsync(Stream content, CancellationToken token)
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

        private static async Task<ItemContent> CreateRawTextItemContentAsync(Encoding encoding, Stream content)
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
    }
}
