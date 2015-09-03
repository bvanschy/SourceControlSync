
namespace SourceControlSync.Domain
{
    public interface IDestinationRepositoryFactory
    {
        IDestinationRepository CreateDestinationRepository(string connectionString);
    }
}
