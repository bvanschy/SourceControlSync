
namespace SourceControlSync.Domain
{
    public interface ISourceRepositoryFactory
    {
        ISourceRepository CreateSourceRepository(string connectionString);
    }
}
