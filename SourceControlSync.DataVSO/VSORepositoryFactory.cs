using SourceControlSync.Domain;

namespace SourceControlSync.DataVSO
{
    public class VSORepositoryFactory : ISourceRepositoryFactory
    {
        public ISourceRepository CreateSourceRepository(string connectionString)
        {
            return new SourceRepository(new DownloadRequest(connectionString));
        }
    }
}
