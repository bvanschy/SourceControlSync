using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain
{
    public interface IRepositoryFactory
    {
        ISourceRepository CreateSourceRepository(string connectionString);
        IDestinationRepository CreateDestinationRepository(string connectionString);
    }
}
