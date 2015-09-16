using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private ItemContent _newContent;

        public ItemChange(ItemChangeType changeType, Item item)
        {
            ChangeType = changeType;
            Item = item;
        }

        public ItemChangeType ChangeType { get; private set; }

        public Item Item { get; private set; }

        public ItemContent NewContent 
        {
            get { return _newContent; }
            set
            {
                if (_newContent != null)
                {
                    throw new InvalidOperationException("NewContent is immutable");
                }
                _newContent = value;
            }
        }

        public Stream CreateContentStream()
        {
            if (NewContent == null)
            {
                throw new InvalidOperationException(string.Format("ItemChange for {0} of type {1} must have content",
                    Item.Path, ChangeType.ToString()));
            }
            if (Item.ContentMetadata == null)
            {
                throw new InvalidOperationException(string.Format("ItemChange for {0} of type {1} must have metadata",
                    Item.Path, ChangeType.ToString()));
            }

            var newContentStream = new MemoryStream();
            NewContent.WriteToStream(newContentStream, Item.ContentMetadata.Encoding);
            newContentStream.Position = 0;
            return newContentStream;
        }

        public async Task SetNewContentAsync(Stream content, CancellationToken token)
        {
            if (Item.ContentMetadata == null)
            {
                throw new InvalidOperationException(string.Format("ItemChange for {0} of type {1} must have metadata",
                    Item.Path, ChangeType.ToString()));
            }

            if (Item.ContentMetadata.IsBinary)
            {
                NewContent = await CreateItemContentFromBinaryStreamAsync(content, token);
            }
            else
            {
                NewContent = await CreateItemContentFromTextStreamAsync(content, Item.ContentMetadata.Encoding);
            }
        }

        private async Task<ItemContent> CreateItemContentFromBinaryStreamAsync(Stream content, CancellationToken token)
        {
            using (var memoryStream = new MemoryStream())
            {
                const int BUFFER_SIZE = 4096;
                await content.CopyToAsync(memoryStream, BUFFER_SIZE, cancellationToken: token);
                var bytes = memoryStream.ToArray();
                return new ItemContent(ItemContentType.Base64Encoded, Convert.ToBase64String(bytes));
            }
        }

        private async Task<ItemContent> CreateItemContentFromTextStreamAsync(Stream content, Encoding encoding)
        {
            using (var streamReader = new StreamReader(content, encoding))
            {
                var contentText = await streamReader.ReadToEndAsync();
                return new ItemContent(ItemContentType.RawText, contentText);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", ChangeType.ToString(), Item.Path);
        }
    }
}
