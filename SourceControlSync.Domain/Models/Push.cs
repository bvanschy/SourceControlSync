using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceControlSync.Domain.Models
{
    public class Push
    {
        public Push(Repository repository, IEnumerable<Commit> commits)
        {
            Repository = repository;
            Commits = commits.ToList();
        }

        public Repository Repository { get; private set; }

        public IEnumerable<Commit> Commits { get; private set; }
    }
}
