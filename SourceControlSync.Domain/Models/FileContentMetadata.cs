using System.Text;

namespace SourceControlSync.Domain.Models
{
    public class FileContentMetadata
    {
        public FileContentMetadata(string contentType, Encoding encoding)
        {
            IsBinary = false;
            ContentType = contentType;
            Encoding = encoding;
        }

        public FileContentMetadata(string contentType)
        {
            IsBinary = true;
            ContentType = contentType;
        }

        public bool IsBinary { get; private set; }

        public string ContentType { get; private set; }

        public Encoding Encoding { get; private set; }
    }
}
